using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

class GestureRecognition
{

    /*
    * Constants
    */
    public const int ANGLE_GRANULARITY = 10;
    public const float PROXIMITY_THRESHOLD = 0.1f;
    public const int MIN_GESTURE_SIZE = 10;
    public const float ANGLE_SUM_THRESHOLD = 0.5f;

    /*
    * Types of gestures.
    */
    public enum Gesture {
        circle_cw,
        circle_ccw,
        hline_lr,
        hline_rl,
        vline_ud,
        vline_du,
        unknown
    };

    /*                                                                                           
    * a data point.                                                                             
    *                                                                                           
    * @param time timestamp since start of program (s)                                          
    * @param x x-coordinate on 2d plane                                                         
    * @param y y-coordinate on 2d plane
    * @param z depth value. 
    */
    public struct Point_2D
    {
        public float time;
        public float x, y, z;
    }

    /*
    * Represents a bounding box.
    */
    public struct Bounds
    {
        public float min_x, min_y, max_x, max_y;
    }

    /*
    * returns Bounds of gesture for subsection described by start & end 
    * parameters.
    */
    private Bounds calculate_Bounds(
            ref List<Point_2D> gesture,
            int start, int end
    )
    {
        Bounds b = new Bounds();
        b.min_x = float.MaxValue;
        b.min_y = float.MaxValue;
        b.max_x = float.MinValue;
        b.max_y = float.MinValue;

        for (int i = start; i < end; i++)
        {
            b.min_x = (gesture[i].x < b.min_x) ? gesture[i].x : b.min_x;
            b.min_y = (gesture[i].y < b.min_y) ? gesture[i].y : b.min_y;
            b.max_x = (gesture[i].x > b.max_x) ? gesture[i].x : b.max_x;
            b.max_y = (gesture[i].y > b.max_y) ? gesture[i].y : b.max_y;
        }

        return b;
    }

    /*
    * Calculates the angles between a gestures data points, and stores 
    * them in @angles. Returns angle sum for subsection described by start
    * & end parameters. @angles should be empty.
    *
    * An angle is the relative angle between two lines AB and BC
    * described by three points A, B and C. In other words, if A, B and C
    * are all points on a single line the angle will be 0. A gesture of n
    * points will have n-2 such angles, as the first and last points only
    * have one connecting edge.
    */
    float calculate_angles(
        ref List<Point_2D> gesture,
        ref List<float> angles,
        Bounds bounds
    )
    {
        float angle_sum = 0;
        angles.Add(0); // pad with 0

        for (int a = 0; a < gesture.Count - 2 * ANGLE_GRANULARITY; a += ANGLE_GRANULARITY)
        {
            int b = a + ANGLE_GRANULARITY;
            int c = a + 2 * ANGLE_GRANULARITY;
            float vec_ab_x = gesture[b].x - gesture[a].x;
            float vec_ab_y = gesture[b].y - gesture[a].y;
            float vec_bc_x = gesture[c].x - gesture[b].x;
            float vec_bc_y = gesture[c].y - gesture[b].y;

            // calculate angle.
            float signed_angle = (float)(Math.Atan2(vec_bc_y, vec_bc_x) -
                    Math.Atan2(vec_ab_y, vec_ab_x));

            // if angle > 180 then angle := 360 - angle
            if (signed_angle > Math.PI)
                signed_angle -= (float)(2 * Math.PI);
            if (signed_angle < -Math.PI)
                signed_angle += (float)(2 * Math.PI);

            // check proximity. If points are too close then skip.
            float proximity_ab = proximity(gesture[a], gesture[b], bounds);
            float proximity_bc = proximity(gesture[b], gesture[c], bounds);
            if (proximity_ab < PROXIMITY_THRESHOLD ||
                    proximity_bc < PROXIMITY_THRESHOLD)
            {
                continue;
            }

            angles.Add(signed_angle);
            angle_sum += signed_angle;
        }

        return angle_sum;
    }

    /*
    * Returns distance between two points relative to size of the gesture.
    */
    float proximity(Point_2D a, Point_2D b, Bounds bounds)
    {
        float gesture_width = bounds.max_x - bounds.min_x;
        float gesture_height = bounds.max_y - bounds.min_y;
        float relative_dx = (b.x - a.x) / gesture_width;
        float relative_dy = (b.y - a.y) / gesture_height;
        float relative_distance = (float)Math.Sqrt(relative_dx * relative_dx
                + relative_dy * relative_dy);
        return relative_distance;
    }

    public Gesture recognize_gesture(List<Point_2D> gesture) {
        Bounds bounds = calculate_Bounds(ref gesture, 0, gesture.Count);
        List<float> angles = new List<float>();
        float angle_sum = calculate_angles(ref gesture, ref angles, bounds);

        // super simple (and ugly) ad hoc classifier
        float gesture_width = bounds.max_x - bounds.min_x;
        float gesture_height = bounds.max_y - bounds.min_y;
        Point_2D start = gesture[0];
        Point_2D end = gesture[gesture.Count - 1];

        Debug.Log("angle_sum: " + angle_sum);
        Debug.Log("gesture_width: " + gesture_width);
        Debug.Log("gesture_height: " + gesture_height);

        // I'm sorry
        if (angle_sum > (2 - ANGLE_SUM_THRESHOLD) * Math.PI && angle_sum < (2 + ANGLE_SUM_THRESHOLD) * Math.PI)
        {
            return Gesture.circle_ccw;
        }
        else if (angle_sum < -(2 - ANGLE_SUM_THRESHOLD) * Math.PI && angle_sum > -(2 + ANGLE_SUM_THRESHOLD) * Math.PI)
        {
            return Gesture.circle_cw;
        }
        else if (Math.Abs(angle_sum) < ANGLE_SUM_THRESHOLD && gesture_width > 4 * gesture_height && gesture_width > 0.15)
        {
            if (start.x > end.x)
            {
                return Gesture.hline_rl;
            }
            else
            {
                return Gesture.hline_lr;
            }
        }
        else if (Math.Abs(angle_sum) < ANGLE_SUM_THRESHOLD && gesture_height > 4 * gesture_width && gesture_height > 0.15)
        {
            if (start.y > end.y)
            {
                return Gesture.vline_ud;
            }
            else
            {
                return Gesture.vline_du;
            }
        }
        else
        {
            return Gesture.unknown;
        }
    }
}

