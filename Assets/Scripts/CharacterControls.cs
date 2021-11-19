using UnityEngine;

public class CharacterControls : MonoBehaviour
{
    
    public float speed;
    public float mouseFollowSpeed;

    public float snapAngle;

    public Transform _camera;

    public Transform _player;
    public CharacterController _controller;
    public Animator _animator;
    public GameObject rifle;
    public GameObject sword;
    public Vector3 playerRot;
    public Transform targetBone;
    public Vector3 boneRot;
    public Vector3 previous;
    public Vector3 velocity;

    private Vector3 mousePosition;
    private Quaternion modelRot;

    private Vector3 moveDirection;

    private Vector3 cameraOffset;

    private bool inExecutionRange = false;
    private bool finisher;
    private Vector3 enemyLocation;
    private EnemyController enemyController;

    private void Start()
    {
        cameraOffset = _camera.position - _player.position;
    }

    private void Update()
    {
        if (inExecutionRange) //If collides with enemy collider
        {
            Execute();
        }
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Finishing")) //check if fininshing animation is playing
        {
            finisher = true;
        }
        else
        {
            inExecutionRange = false;
            finisher = false;
        }
        ChangeWeapon(finisher);
    }

    public void LateUpdate()
    {
        if (!finisher)
        {
            //Get input
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 input = new Vector3(horizontal, 0, vertical).normalized;

            //Shoot ray from camera to world based on mouse screen position
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Camera.main.farClipPlane))
            {
                if (hit.collider.tag == "Plane")
                {
                    mousePosition = hit.point; //Get world position of ray hit point
                }
            }

            Debug.DrawRay(ray.origin, ray.direction * Camera.main.farClipPlane, Color.red); //Draws ray in scene view

            mousePosition.y = 0;
            Vector3 modelDirection = mousePosition - transform.position;
            modelDirection.y = 0;
            Quaternion modelRotation = Quaternion.LookRotation(modelDirection); //Sets rotation direction based on ray hit and player transform

            modelRot = Quaternion.Lerp(modelRot, modelRotation, mouseFollowSpeed * Time.deltaTime);
            boneRot = new Vector3(0, modelRot.eulerAngles.y - 90, -84.48f); //Sets bone rotation with pelvis rotation offsets

            //Moves character based on screen rotation
            if (input.magnitude > 0)
            {
                float targetAngle = Mathf.Atan2(input.x, input.z) * Mathf.Rad2Deg + _camera.eulerAngles.y;

                moveDirection = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
                transform.rotation = modelRot;
                _controller.Move(moveDirection.normalized * speed * Time.deltaTime);
            }
            else //Rotates only spine while player not moving and corrects whole model rotation if spine rotation.x is more then desired snapAngle
            {
                _controller.Move(Vector3.zero);
                targetBone.eulerAngles = boneRot;
                if (targetBone.localEulerAngles.x > snapAngle && targetBone.localEulerAngles.x < 360 - snapAngle)
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, modelRot, mouseFollowSpeed * Time.deltaTime);
                    modelRot = Quaternion.Lerp(modelRot, modelRotation, mouseFollowSpeed * Time.deltaTime);
                    boneRot = new Vector3(0, modelRot.eulerAngles.y - 90, -84.48f);
                    targetBone.eulerAngles = boneRot;
                }
            }


            //Calculates player velocity
            Vector3 newCameraPos = _player.position + cameraOffset;
            _camera.position = newCameraPos;

            velocity = (transform.position - previous) / Time.deltaTime;
            previous = transform.position;

            //Converst velocity to localAxis
            float velocityZ = transform.InverseTransformDirection(velocity).normalized.z;
            float velocityX = transform.InverseTransformDirection(velocity).normalized.x;

            _animator.SetFloat("VelocityZ", velocityZ, 0.25f, Time.deltaTime);
            _animator.SetFloat("VelocityX", velocityX, 0.25f, Time.deltaTime); //Passes velocity to animator blend tree
        }
        else //Rotates player towards enemy if in Finishing animation
        {
            targetBone.localEulerAngles = Vector3.zero;
            Vector3 modelDirection = enemyLocation - transform.position;
            modelDirection.y = 0;
            Quaternion modelRotation = Quaternion.LookRotation(modelDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, modelRotation, mouseFollowSpeed * Time.deltaTime * 2);
        }
    }

    private void Execute() //Sets animation trigger
    {
        if (Input.GetKeyDown(KeyCode.F) && !finisher)
        {
            _animator.SetTrigger("Finisher");
            finisher = true;
            enemyController.colOnOff = true;
        }
    }

    private void ChangeWeapon(bool f) //Sets weapon if finishing
    {
        if (!f)
        {
            rifle.SetActive(true);
            sword.SetActive(false);
        }
        else
        {
            rifle.SetActive(false);
            sword.SetActive(true);
        }
    }

    private void OnTriggerStay(Collider other) //Check if in range to execute
    {
        if(other.tag == "Enemy")
        {
            inExecutionRange = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy") //Gets enemy position and controller
        {
            enemyLocation = other.transform.position;
            enemyController = other.gameObject.GetComponent<EnemyController>();
        }
    }
}
