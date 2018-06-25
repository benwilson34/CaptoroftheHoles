using GamepadInput;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {

    public const float JUMP_FORCE = 15f;
    private const float FALL_GRAV = 68f, HIGHJUMP_GRAV = 35f;
    public float currentGrav = FALL_GRAV;
    //public bool wasTeleported = false, readyToTeleport = true;
    public Main.Team team = Main.Team.Red;
    GamePad.Index controllerNum = GamePad.Index.One;

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

    private enum Direction { None = -1, Right, UpRight, Up, UpLeft, Left, DownLeft, Down, DownRight };
    private Direction _lastDirection, _directionTwoFramesAgo;
    private Direction _lastLeftOrRight = Direction.Right;
    private bool _facingLeft = false;

    public GameObject bulletPF;
    public Safehouse safehouse;

    private Maiden _maiden;
    public bool CarryingMaiden { get { return _maiden != null; } }

    private bool _isAlive = true;

    void Start () {
        _rb = GetComponent<Rigidbody2D>();
        _rb.velocity = new Vector2(0, -1);
        Debug.Log(_rb.velocity.ToString());

        if (team == Main.Team.Blue)
            controllerNum = GamePad.Index.Two;

        _groundCheck = transform.Find("groundCheck");
        _lastFrameState = GamePad.GetState(GamePad.Index.One);
    }

    void FixedUpdate () {
        UpdateGrav();

        if (!_isAlive)
            return;

        GamepadState state = GamePad.GetState(controllerNum);

        Direction aimDirection = HandleDpad(state);
        HandleAButton(GetButtonState(state, GamePad.Button.A));
        HandleXButton(GetButtonState(state, GamePad.Button.Y), aimDirection);
        HandleR2(GetButtonState(state, GamePad.Button.RightShoulder));

        HandleTouches();

        _lastFrameState = state;
        //Debug.Log("last left or right = " + _lastLeftOrRight);

        if (!CanTeleport) {
            _cooldown -= PORTAL_COOLDOWN * Time.deltaTime;
        }
    }

    Direction HandleDpad(GamepadState state) {
        HandleMovement(state.dPadAxis);

        Direction dir = GetDirection(state);
        if (dir != GetDirection(_lastFrameState)) {
            if (dir == Direction.None) {
                var lastLeftOrRight = GetLeftOrRight(_lastDirection);
                if (lastLeftOrRight == Direction.None) // only up or down i think
                    lastLeftOrRight = GetLeftOrRight(_directionTwoFramesAgo);
                AimInDirection(lastLeftOrRight);
                _directionTwoFramesAgo = _lastDirection;
                _lastDirection = lastLeftOrRight;
            } else {
                AimInDirection(dir);
                _directionTwoFramesAgo = _lastDirection;
                _lastDirection = dir;
                Debug.Log(dir.ToString());
            }
        }

        return _lastDirection;
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

    void AimInDirection(Direction dir) {
        var pivot = transform.Find("armPivot");
        //var arm = pivot.Find("arm");

        float angle = ProperAimAngle(dir);
        pivot.rotation = Quaternion.Euler(0, 0, angle);

        // flip if needed
        if (dir != Direction.Up && dir != Direction.Down) {
            Vector3 scale = Vector3.one;
            scale.x = GetLeftOrRight(dir) == Direction.Right ? 1 : -1;
            transform.localScale = scale;
        }
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

    float ProperAimAngle(Direction dir) {
        Debug.Log("Aiming to " + dir.ToString());
        switch (dir) {
            case Direction.Down:
                return 90 * 
                    (GetLeftOrRight(_lastDirection) == Direction.Left ? 1 : -1);
            case Direction.Up:
                return 90 * 
                    (GetLeftOrRight(_lastDirection) == Direction.Left ? -1 : 1);
            case Direction.Left:
                return DirToAngle(Direction.Right);
            case Direction.UpLeft:
                return DirToAngle(Direction.DownRight);
            case Direction.DownLeft:
                return DirToAngle(Direction.UpRight);
            default:
                return DirToAngle(dir);
        }
    }

    float DirToAngle(Direction dir) {
        return (float)dir * 45f;
    }

    void UpdateGrav() {
        //if (_groundCheckFrames == 0)
            _grounded = Physics2D.Linecast(transform.position,
                _groundCheck.position, 1 << LayerMask.NameToLayer("Ground"));
        //else
        //    _groundCheckFrames--;

        float velY = _rb.velocity.y;
        //Debug.Log(velY);

        //if (Mathf.Abs(velY) < 0.005f) // TODO keep track of last frame for optimization
        //    velY = 0;
        //else {
        if (!_grounded) {
            velY -= currentGrav * Time.deltaTime;
        }

       

        //Debug.Log("y vel="+vel);
        _rb.velocity = new Vector2(_rb.velocity.x, velY);
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
                currentGrav = HIGHJUMP_GRAV;
                break;

            case ButtonState.Hold:
                //const float jumpDecay = -40;
                //if (_jumpVelocity > 1f) {
                //    _jumpVelocity += jumpDecay * Time.deltaTime;
                //    _rb.velocity = new Vector2(_rb.velocity.x, _jumpVelocity);
                //} else {
                //    _jumpVelocity = 0;
                //}
                break;

            case ButtonState.Up:
                _jump = false;
                currentGrav = FALL_GRAV;
                break;

            default:
                break;
        }
    }

    void HandleXButton(ButtonState state, Direction aimDirection) {
        switch (state) {
            case ButtonState.None:
                break;

            case ButtonState.Down:
                Debug.Log("X button");
                var bullet = Instantiate(bulletPF).GetComponent<Bullet>();
                Transform pivot = transform.Find("armPivot");
                Transform bulletSpawn = pivot.Find("bulletSpawn");
                //Transform gunback = pivot.Find("gunBack");
                //bullet.moveDirection = bulletSpawn.position - gunback.position;
                bullet.Init(team, DirToAngle(aimDirection));
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

    public void Die() {
        if (_isAlive) {
            _isAlive = false;
            StartCoroutine(_DeathAnim());
        }
    }
    IEnumerator _DeathAnim() {
        const float respawnTime = 2f; // 2 second respawn time
        //var rend = GetComponent<SpriteRenderer>();
        //rend.color = new Color(1, 1, 1, 0.5f);
        yield return new WaitForSeconds(respawnTime/2);
        //rend.enabled = false;
        yield return new WaitForSeconds(respawnTime/2);
        Main.GetSafehouse(team).SpawnPlayer();
        Destroy(this.gameObject);
    }
}
