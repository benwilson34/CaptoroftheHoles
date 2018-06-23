using GamepadInput;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {

    public const float JUMP_FORCE = 15f;
    public float grav = -100f;
    //public bool wasTeleported = false, readyToTeleport = true;
    public Main.Side screenSide = Main.Side.Left;

    private enum ButtonState { None, Down, Hold, Up };
    private Rigidbody2D _rb;
    private GamepadState _lastFrameState;
    public float moveForce = 365f;
    public float maxSpeed = 5f;
    private bool _jump = false, _grounded = false;
    private Transform _groundCheck;
    private int _groundCheckFrames = 0;
    private float _jumpVelocity;

    private const float PORTAL_COOLDOWN = 1;
    private float _cooldown;
    public bool CanTeleport { get { return _cooldown < 0.005f; } }

    private enum Direction { None, Up, UpRight, Right, DownRight, Down, DownLeft, Left, UpLeft };
    private Direction _lastDirection;
    private Direction _lastLeftOrRight = Direction.Right;
    private bool _facingLeft = false;

    public GameObject bulletPF;
    public Safehouse safehouse;

    private Maiden _maiden;
    public bool CarryingMaiden { get { return _maiden != null; } }

    void Start () {
        _rb = GetComponent<Rigidbody2D>();
        _groundCheck = transform.Find("groundCheck");
        _lastFrameState = GamePad.GetState(GamePad.Index.One);
    }

    void FixedUpdate () {
        UpdateGrav();

        GamePad.Index controllerNum = GamePad.Index.One;
        if (screenSide == Main.Side.Right)
            controllerNum = GamePad.Index.Two;

        GamepadState state = GamePad.GetState(controllerNum);

        Vector2 movement = GamePad.GetAxis(GamePad.Axis.Dpad, controllerNum);
        HandleMovement(movement);
        HandleAim(state);

        HandleAButton(GetButtonState(state, GamePad.Button.A));
        HandleXButton(GetButtonState(state, GamePad.Button.Y));
        HandleR2(GetButtonState(state, GamePad.Button.RightShoulder));

        HandleTouches();

        _lastFrameState = state;
        //Debug.Log("last left or right = " + _lastLeftOrRight);

        if (!CanTeleport) {
            _cooldown -= PORTAL_COOLDOWN * Time.deltaTime;
        }
    }

    void HandleMovement(Vector2 movement) {
        float h = movement.x;
        Vector2 vel = _rb.velocity;
        const float moveSpeed = 300f;
        if (Mathf.Abs(h) > 0.001f) {
            vel.x = h * moveSpeed * Time.deltaTime;
        } else {
            // decay movement
            float friction = 50f * Time.deltaTime;
            if (Mathf.Abs(vel.x) > 1)
                vel.x = Mathf.Sign(vel.x) * (Mathf.Abs(vel.x) - friction);
            else
                vel.x = 0;
        }

        _rb.velocity = vel;
    }

    void HandleAim(GamepadState state) {
        Direction dir = GetDirection(state);
        if (_lastDirection == dir)
            return;

        AimInDirection(dir);

        _lastDirection = dir;
    }

    void AimInDirection(Direction dir) {
        var pivot = transform.Find("armPivot");
        var arm = pivot.Find("arm");
        switch (dir) {
            case Direction.None:
                AimInDirection(_lastLeftOrRight);
                break;
            case Direction.Up:
                pivot.rotation = Quaternion.Euler(new Vector3(0, pivot.rotation.y, 90));
                break;
            case Direction.UpRight:
                pivot.rotation = Quaternion.Euler(new Vector3(0, pivot.rotation.y, 45));
                break;
            case Direction.Right:
                pivot.rotation = Quaternion.Euler(new Vector3(0, pivot.rotation.y, 0));
                break;
            case Direction.DownRight:
                pivot.rotation = Quaternion.Euler(new Vector3(0, pivot.rotation.y, -45));
                break;
            case Direction.Down:
                pivot.rotation = Quaternion.Euler(new Vector3(0, pivot.rotation.y, -90));
                break;
            case Direction.DownLeft:
                pivot.rotation = Quaternion.Euler(new Vector3(0, pivot.rotation.y, 135));
                break;
            case Direction.Left:
                pivot.rotation = Quaternion.Euler(new Vector3(0, pivot.rotation.y, 180));
                break;
            case Direction.UpLeft:
                pivot.rotation = Quaternion.Euler(new Vector3(0, pivot.rotation.y, 225));
                break;
            default:
                break;
        }

        Direction leftOrRight = GetLeftOrRight(dir);
        //Debug.Log("leftOrRight =" + leftOrRight.ToString());
        //if (leftOrRight == Direction.Left) {
        //    transform.localScale = new Vector3(-1, 1, 1);
        //} else if (leftOrRight == Direction.Right) {
        //    transform.localScale = new Vector3(1, 1, 1);
        //}
        if(leftOrRight == Direction.Left || leftOrRight == Direction.Right)
            _lastLeftOrRight = leftOrRight;
    }

    Direction GetDirection(GamepadState state) {
        if (state.Left) {
            if (state.Up) {
                return Direction.UpLeft;
            } else if (state.Down) {
                return Direction.DownLeft;
            } else return Direction.Left;
        } else if (state.Right) {
            if (state.Up) {
                return Direction.UpRight;
            } else if (state.Down) {
                return Direction.DownRight;
            } else return Direction.Right;
        } else {
            if (state.Up) {
                return Direction.Up;
            } else if (state.Down) {
                return Direction.Down;
            } else return Direction.None;
        }
    }

    Direction GetLeftOrRight(Direction dir) {
        if (dir == Direction.DownLeft ||
            dir == Direction.Left ||
            dir == Direction.UpLeft) {
            return Direction.Left;
        } else if (dir == Direction.DownRight ||
            dir == Direction.Right ||
            dir == Direction.UpRight) {
            return Direction.Right;
        } else return Direction.None;
    }

    //void SwitchSidesIfNeeded(Direction dir) {
    //    Direction lastLeftOrRight = GetLeftOrRight(_lastDirection);
    //    Direction thisLeftOrRight = GetLeftOrRight(dir);
    //    if (thisLeftOrRight == Direction.None)
    //        return;

    //    if(lastLeftOrRight != Direction.None)
    //        _lastLeftOrRight = thisLeftOrRight;

    //    if (thisLeftOrRight != lastLeftOrRight) {
    //        Debug.Log("Flipping....");
    //        // TODO flip sprites
    //        //transform.Rotate(0, 180, 0);
    //    }
    //}

    void UpdateGrav() {
        //if (_groundCheckFrames == 0)
            _grounded = Physics2D.Linecast(transform.position,
                _groundCheck.position, 1 << LayerMask.NameToLayer("Ground"));
        //else
        //    _groundCheckFrames--;

        float vel = _rb.velocity.y;

        if (_grounded) // TODO keep track of last frame for optimization
            vel = 0;
        else {
            vel += grav * Time.deltaTime;
        }

        //Debug.Log("y vel="+vel);
        _rb.velocity = new Vector2(_rb.velocity.x, vel);
    }

    void HandleTouches() {
        bool portal = Physics2D.Linecast(transform.position,
                _groundCheck.position, 1 << LayerMask.NameToLayer("Portal"));
        if (portal) {
            //Debug.Log("touched portal");
        }

        bool maiden = Physics2D.Linecast(transform.position,
                _groundCheck.position, 1 << LayerMask.NameToLayer("Maiden"));
        if (maiden) {
            //Debug.Log("touched maiden");
        }
    }

    public void SetMaiden(Maiden maiden) {
        _maiden = maiden;
        _maiden.transform.SetParent(transform);
        _maiden.transform.position = transform.Find("maidenSpot").position;
    }

    public void DropMaiden() {
        
    }

    public void ClearMaiden() {
        Destroy(_maiden.gameObject);
        _maiden = null;
    }

    public void Teleported() {
        _cooldown = 1;
    }

    ButtonState GetButtonState(GamepadState currentState, GamePad.Button button) {
        bool prevDown = _lastFrameState.GetButtonState(button);
        bool currentDown = currentState.GetButtonState(button);
        if (!prevDown && currentDown)
            return ButtonState.Down;
        else if (prevDown && currentDown)
            return ButtonState.Hold;
        else if (prevDown && !currentDown)
            return ButtonState.Up;
        else
            return ButtonState.None;
    }

    void HandleAButton(ButtonState state) {
        if (!_jump && !_grounded)
            return;

        switch (state) {
            case ButtonState.None:
                break;

            case ButtonState.Down:
                Debug.Log("A button");
                if (!_grounded) // needed here?
                    return;
                _jump = true;
                _groundCheckFrames = 5;
                _grounded = false;
                _jumpVelocity = JUMP_FORCE;
                _rb.velocity = new Vector2(_rb.velocity.x, _jumpVelocity);
                break;

            case ButtonState.Hold:
                const float jumpDecay = -40;
                if (_jumpVelocity > 1f) {
                    _jumpVelocity += jumpDecay * Time.deltaTime;
                    _rb.velocity = new Vector2(_rb.velocity.x, _jumpVelocity);
                } else {
                    _jumpVelocity = 0;
                }
                break;

            case ButtonState.Up:
                _jump = false;
                break;

            default:
                break;
        }
    }

    void HandleXButton(ButtonState state) {
        switch (state) {
            case ButtonState.None:
                break;

            case ButtonState.Down:
                Debug.Log("X button");
                var bullet = Instantiate(bulletPF).GetComponent<Bullet>();
                Transform pivot = transform.Find("armPivot");
                Transform bulletSpawn = pivot.Find("bulletSpawn");
                Transform gunback = pivot.Find("gunBack");
                bullet.moveDirection = bulletSpawn.position - gunback.position;
                bullet.side = screenSide;
                bullet.transform.position = bulletSpawn.position;
                break;

            case ButtonState.Hold:
                break;

            case ButtonState.Up:
                break;

            default:
                break;
        }
    }

    void HandleR2(ButtonState state) {
        switch (state) {
            case ButtonState.None:
                break;

            case ButtonState.Down:
                //bool maiden = Physics2D.Linecast(transform.position,
                //    _groundCheck.position, 1 << LayerMask.NameToLayer("Maiden"));
                //if (maiden) {
                //    Debug.Log("touched maiden");
                //}
                break;

            case ButtonState.Hold:
                break;

            case ButtonState.Up:
                break;

            default:
                break;
        }
    }
}
