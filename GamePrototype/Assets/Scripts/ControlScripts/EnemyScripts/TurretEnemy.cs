using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretEnemy : MonoBehaviour
{
    public GameObject targetLocation; // target area
    public GameObject ammoSpawn; // we spawn ammo here
    public GameObject ammo; // red ball ammo prefab
    public GameObject gunRotator; // this will rotate the gun
    public float force; // the force we will shoot. The force will always be the same, we will adjust the angle
    public Vector3 gravity;
    private int angleMultiplier;

    void Start()
    {
        gravity = Physics.gravity;
    }

    void Update()
    {
        
    }

    public void Shoot()
    {
        StartCoroutine(ShootBalls());
    }

    IEnumerator ShootBalls()
    {
        Debug.Log("Shooting");
        Vector3[] direction = HitTargetBySpeed(ammoSpawn.transform.position, targetLocation.transform.position, gravity, force);

        // this fixes the gun to rotate down when target z is smaller than enemy z
        if(gameObject.transform.position.z < targetLocation.transform.position.z)
        {
            angleMultiplier = -1;
        }
        else
        {
            angleMultiplier = 1;
        }


        gunRotator.GetComponent<GunRotator>().xAngle = Mathf.Atan(direction[0].y / direction[0].z) * Mathf.Rad2Deg * angleMultiplier;

        yield return new WaitUntil(() => gunRotator.GetComponent<GunRotator>().rotating == false);


        GameObject projectile = Instantiate(ammo, ammoSpawn.transform.position, Quaternion.identity);
        projectile.GetComponent<Rigidbody>().AddRelativeForce(direction[0], ForceMode.Impulse);

        yield return new WaitForSecondsRealtime(2);


        gunRotator.GetComponent<GunRotator>().xAngle = Mathf.Atan(direction[1].y / direction[1].z) * Mathf.Rad2Deg * angleMultiplier;

        yield return new WaitUntil(() => gunRotator.GetComponent<GunRotator>().rotating == false);


        GameObject projectile2 = Instantiate(ammo, ammoSpawn.transform.position, Quaternion.identity);
        projectile2.GetComponent<Rigidbody>().AddRelativeForce(direction[1], ForceMode.Impulse);

    }

    public Vector3[] HitTargetBySpeed(Vector3 startPosition, Vector3 targetPosition, Vector3 gravityBase, float launchSpeed)
    {
        Vector3 AtoB = targetPosition - startPosition;
        Vector3 horizontal = GetHorizontalVector(AtoB, gravityBase, startPosition);
        Vector3 vertical = GetVerticalVector(AtoB,gravityBase, startPosition);

        float horizontalDistance = horizontal.magnitude;
        float verticalDistance = vertical.magnitude * Mathf.Sign(Vector3.Dot(vertical, - gravityBase));

        float x2 = horizontalDistance * horizontalDistance;
        float v2 = launchSpeed * launchSpeed;
        float v4 = launchSpeed * launchSpeed * launchSpeed * launchSpeed;
        float gravMag = gravity.magnitude;

        float launchTest = v4 - (gravMag* ((gravMag * x2) + ( 2 * verticalDistance)));

        Debug.Log("LAUNCHTEST: " + launchTest);

        Vector3[] launch = new Vector3[2];

        if(launchTest < 0)
        {
            Debug.Log("We cannot hit the target but we let's shoot 2 45 degree balls anyway");
            launch[0] = (horizontal.normalized * launchSpeed * Mathf.Cos(45.0f *  Mathf.Deg2Rad))
                - gravityBase.normalized * launchSpeed * Mathf.Sin(45.0f * Mathf.Deg2Rad);

            launch[0] = launch[1];
        }
        else
        {
            Debug.Log("We can hit the target, let's calculate the angles");
            float[] tanAngle = new float[2];
            tanAngle[0] = (v2 - Mathf.Sqrt(v4 - gravMag * ((gravMag * x2) + (2 * verticalDistance * v2)))) / (gravMag * horizontalDistance);
            tanAngle[1] = (v2 + Mathf.Sqrt(v4 - gravMag * ((gravMag * x2) + (2 * verticalDistance * v2)))) / (gravMag * horizontalDistance);

            float[] finalAngle = new float[2];

            finalAngle[0] = Mathf.Atan(tanAngle[0]);
            finalAngle[1] = Mathf.Atan(tanAngle[1]);

            Debug.Log("Then angles we need to shoot are: " + finalAngle[0] * Mathf.Rad2Deg + " and " + finalAngle[1] * Mathf.Rad2Deg);

            launch[0] = (horizontal.normalized * launchSpeed * Mathf.Cos(finalAngle[0])) -
                gravityBase.normalized * launchSpeed * Mathf.Sin(finalAngle[0]);

            launch[1] = (horizontal.normalized * launchSpeed * Mathf.Cos(finalAngle[1])) -
                gravityBase.normalized * launchSpeed * Mathf.Sin(finalAngle[1]);

        }

        return launch;


    }

    public Vector3 GetHorizontalVector(Vector3 AtoB, Vector3 gravityBase, Vector3 startPosition)
    {
        Vector3 output;
        Vector3 perpendicular = Vector3.Cross(AtoB, gravityBase);
        perpendicular = Vector3.Cross(gravityBase, perpendicular); // Vector pointing horizontal direction
        output = Vector3.Project(AtoB, perpendicular); // this horizontal vector
        Debug.DrawRay(startPosition, output, Color.green, 10f); // draw the Vector on screen

        return output;
    }

    public Vector3 GetVerticalVector(Vector3 AtoB, Vector3 gravityBase, Vector3 startPosition)
    {
        Vector3 output;
        output = Vector3.Project(AtoB, gravityBase); // this horizontal vector
        Debug.DrawRay(startPosition, output, Color.red, 10f); // draw the Vector on screen
        return output;
    }

}
