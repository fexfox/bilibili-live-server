using UnityEngine;

namespace CreativeVeinStudio.Simple_Pool_Manager.Extras
{
    /// <summary>
    /// LocomotionActions contains a list of general locomotion movements
    /// </summary>
    public static class LocomotionActions
    {
        #region Interactions
        /// <summary>
        /// This function returns true or false whether the target is within the viewAngle
        /// </summary>
        /// <param name="currentPos"></param>
        /// <param name="targetPos"></param>
        /// <param name="viewAngle"></param>
        /// <returns>Boolean</returns>
        public static bool TargetWithinViewAngle(ref Transform currentPos, ref Transform targetPos,  float viewAngle)
        {
            return Vector3.Angle((targetPos.position - currentPos.position), currentPos.forward) < viewAngle;
        }
        
        /// <summary>
        /// Turns the provided Transform on the given degreeToRotate by set angle and speed
        /// </summary>
        /// <param name="objBeingTurned"></param>
        /// <param name="axisToTurnOn"></param>
        /// <param name="angleToTurnBy"></param>
        /// <param name="speed"></param>
        public static void TurnBySetAngle(ref Transform objBeingTurned, AxisDirection axisToTurnOn, float angleToTurnBy, float speed)
        {
            var currentAngle = Vector3.zero;
            switch (axisToTurnOn)
            {
                case AxisDirection.xAxis:
                    currentAngle.x = Mathf.MoveTowardsAngle(objBeingTurned.eulerAngles.x, angleToTurnBy, speed * Time.deltaTime);
                    break;
                case AxisDirection.yAxis:
                    currentAngle.y = Mathf.MoveTowardsAngle(objBeingTurned.eulerAngles.y, angleToTurnBy, speed * Time.deltaTime);
                    break;
                case AxisDirection.zAxis:
                    currentAngle.z = Mathf.MoveTowardsAngle( objBeingTurned.eulerAngles.z, angleToTurnBy, speed * Time.deltaTime);
                    break;
            }
            
            objBeingTurned.eulerAngles = currentAngle;
        }

        /// <summary>
        /// Moves the given RigidBody on the X degreeToRotate by the speed and direction provided
        /// </summary>
        /// <param name="moveDir"></param>
        /// <param name="moveSpeed"></param>
        /// <param name="rigidB"></param>
        public static void MoveByPos2D(ref Vector3 moveDir, float moveSpeed, ref Rigidbody rigidB)
        {
            var move = rigidB.transform.position + new Vector3(moveDir.x, 0, 0) * moveSpeed * Time.deltaTime;
            rigidB.MovePosition(move);
        }

        /// <summary>
        /// Moves the given Rigidbody by the speed and direction provided
        /// </summary>
        /// <param name="moveDir"></param>
        /// <param name="moveSpeed"></param>
        /// <param name="rigidB"></param>
        public static void MoveByVelocity(ref Vector3 moveDir, float moveSpeed, ref Rigidbody rigidB)
        {
            var moveToPos = moveDir * moveSpeed * Time.deltaTime;
            rigidB.velocity = moveToPos;
        }

        /// <summary>
        /// Turns the given Rigidbody on the y degreeToRotate by 90 degree, depending on the direction provided. Generally the HORIZONTAL input is provided
        /// </summary>
        /// <param name="moveDir"></param>
        /// <param name="rigidB"></param>
        public static void TurnLeftRight2D(ref Vector3 moveDir, ref Rigidbody rigidB)
        {
            if (moveDir == Vector3.zero) return;
            Quaternion rotateTo = Quaternion.Euler(new Vector3(0, moveDir.x > 0 ? 90 : -90, 0));
            rigidB.rotation = rotateTo;
        }

        /// <summary>
        /// Turns the given Rigidbody the y degreeToRotate by 90 degree, depending on the direction provided, using LERP. Generally the HORIZONTAL input is provided
        /// </summary>
        /// <param name="moveDir"></param>
        /// <param name="turnSpeed"></param>
        /// <param name="rigidB"></param>
        public static void TurnWithLerp2D(ref Vector3 moveDir, float turnSpeed, ref Rigidbody rigidB)
        {
            if (moveDir == Vector3.zero) return;
            Quaternion rotateTo = Quaternion.Euler(new Vector3(0, moveDir.x > 0 ? 90 : -90, 0));
            var lerpTo = Quaternion.Lerp(rigidB.rotation, rotateTo, turnSpeed);
            rigidB.rotation = lerpTo;
        }

        /// <summary>
        /// Jump with Physics.
        /// </summary>
        /// <param name="jumpForce"></param>
        /// <param name="rigidB"></param>
        public static void JumpPhysics(float jumpForce, ref Rigidbody rigidB)
        {
            rigidB.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        /// <summary>
        /// Performs a jump by adjusting the velocity on the Vector3 up value. Works best when used with <see cref="AddGravity"/>
        /// </summary>
        /// <param name="jumpForce"></param>
        /// <param name="rigidB"></param>
        public static void JumpByVelocity(float jumpForce, ref Rigidbody rigidB )
        {
            rigidB.velocity += Vector3.up * jumpForce * Time.deltaTime;
        }
        
        /// <summary>
        /// Sets the gravity for the given object using a preset gravity. The multiplier adds additional force down for that fine tuning. Uses the PHYSICS gravity y value of -9.81
        /// </summary>
        /// <param name="gravityDir"></param>
        /// <param name="gravity"></param>
        /// <param name="gravityMultiplier"></param>
        /// <param name="rigidB"></param>
        public static void AddGravity(ref Vector3 gravityDir, ref float gravityMultiplier, ref Rigidbody rigidB)
        {
            var gravity = Physics.gravity.y;
            
            Vector3 val = gravityDir * -Mathf.Abs(gravity) * gravityMultiplier;
            rigidB.AddForce(val, ForceMode.Force);
        }
        
        public static void RotateTowards2D(this Transform trans, Vector3 to, float speed, bool loop = false)
        {
            //get the direction of the other object from current object
            Vector3 dir = to - trans.position;
            //get the angle from current direction facing to desired target
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            //set the angle into a quaternion + sprite offset depending on initial sprite facing direction

            if (trans.rotation.z >= angle)
            {
                Quaternion rotation = Quaternion.Euler(new Vector3(0, 0, -angle));
                //Roatate current game object to face the target using a slerp function which adds some smoothing to the move
                trans.rotation = Quaternion.Slerp(trans.rotation, rotation, speed * Time.deltaTime);
            }
            else
            {
                Quaternion rotation = Quaternion.Euler(new Vector3(0, 0, angle));
                //Roatate current game object to face the target using a slerp function which adds some smoothing to the move
                trans.rotation = Quaternion.Slerp(trans.rotation, rotation, speed * Time.deltaTime);
            }
        }

        public static void PingPongObjectByAxis(ref Transform trans, Vector3 degreeToRotate, Quaternion startRot, float speed){
            var currentEulerAngles = new Vector3(
                                startRot.eulerAngles.x + (degreeToRotate.x > 0 ? Mathf.PingPong(Time.realtimeSinceStartup * speed, Mathf.Abs(degreeToRotate.x)) : 0),
                                startRot.eulerAngles.y + (degreeToRotate.y > 0 ? Mathf.PingPong(Time.realtimeSinceStartup * speed, Mathf.Abs(degreeToRotate.y)) : 0),
                                startRot.eulerAngles.z + (degreeToRotate.z > 0 ? Mathf.PingPong(Time.realtimeSinceStartup * speed, Mathf.Abs(degreeToRotate.z)) : 0));

            trans.localRotation = Quaternion.Euler(currentEulerAngles);
        }

        public static void RotateOnSpecifiedAxis(ref Transform trans, Vector3 axis, float speed){
            var currentEulerAngles = new Vector3(
                axis.x * speed * Time.deltaTime,
                axis.y * speed * Time.deltaTime,
                axis.z * speed * Time.deltaTime);

            trans.Rotate(currentEulerAngles);
        }
        #endregion
    }

    public enum AxisDirection
    {
        xAxis,
        yAxis,
        zAxis
    }
}