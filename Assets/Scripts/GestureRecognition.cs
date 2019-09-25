using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

public class GestureRecognition
{

    /*
     * Constants
     */
    public const int ANGLE_GRANULARITY = 10;
    public const float PROXIMITY_THRESH = 0.2f;
    public const int MIN_GESTURE_SIZE = 10;
    public const float ANGLE_SUM_THRESH = 0.5f;
    public const float VEL_VEC_NORM_THRESH = 0.25f;
    public const float ANGLE_DEVIATION_THRESH = 0.5f;
    public const bool LOWPASS = false;
    public const int LP_KERNEL_SIZE = 2;
    public const float LP_SIGMA = 2.0f;

    /*
     * The types of gestures.
     */
    public enum Gesture
    {
        circle_cw,
        circle_ccw,
        hline_lr,
        hline_rl,
        vline_ud,
        vline_du,
        push,
        pull,
        unknown
    };

    /*
     * Contains information about the performed gesture.
     */
    public struct Gesture_Meta
    {
        public Gesture type;
        public float angle_sum;
        public Point_3D avg_vel_vector;
        public Bounds bounds;
    }

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
        public Point_2D(float time, float x, float y, float z) {
            this.time = time;
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    /*
     * a 3D point in world space.
     *
     * @param time timestamp since start of program (s)
     * @param x x-coordinate
     * @param y y-coordinate
     * @param z z-coordinate
     */
    public struct Point_3D
    {
        public float time;
        public float x, y, z;
    }

    private Point_2D normalize(Point_2D vec2, bool ignore_z) {
        Point_2D ret;
        float norm = vec2_norm(vec2, ignore_z);
        ret.time = vec2.time;
        ret.x = vec2.x / norm;
        ret.y = vec2.y / norm;
        ret.z = vec2.z / ((ignore_z) ? 1 : norm);
        return ret; 
    }

    private float vec3_norm(Point_3D vec)
    {
        return (float)Math.Sqrt(vec.x * vec.x + vec.y * vec.y + vec.z * vec.z);
    }

    private float vec2_norm(Point_2D vec, bool ignore_z)
    {
        return (float)Math.Sqrt(
                vec.x * vec.x +
                vec.y * vec.y +
                ((ignore_z) ? 0 : vec.z * vec.z)
        );
    }

    /*
     * Represents a bounding box.
     */
    public struct Bounds
    {
        public float min_x, min_y, max_x, max_y, min_z, max_z;
    }

    /*
     * Returns the mean velocity vector for the gesture in screen space.
     */
    private Point_2D mean_velocity_2D(ref List<Point_2D> gesture)
    {
        Point_2D v = new Point_2D();
        v.time = 0;
        v.x = 0;
        v.y = 0;
        v.z = 0;
        for (int i = 0; i < gesture.Count - 1; i++)
        {
            float dx = gesture[i + 1].x - gesture[i].x;
            float dy = gesture[i + 1].y - gesture[i].y;
            float dz = gesture[i + 1].z - gesture[i].z;
            float dt = gesture[i + 1].time - gesture[i].time;
            v.x += dx / dt;
            v.y += dy / dt;
            v.z += dz / dt;
        }

        v.x /= (gesture.Count - 1);
        v.y /= (gesture.Count - 1);
        v.z /= (gesture.Count - 1);

        return v;
    }

    /*
     * Optional low pass filter. Averages sequences of 2d points with
     * weights calculated from Gaussian function. Basically 1D Gaussian
     * blur. Crops edges.
     */
    private List<Point_2D> gaussian_average_lowpass(
            List<Point_2D> gesture,
            int kernel_size,
            float sigma
    )
    {
        List<Point_2D> output = new List<Point_2D>();
        double denom = Math.Sqrt(2 * Math.PI) * sigma;
        double[] kernel = new double[2 * kernel_size + 1];
        double kernel_sum = 0.0f;

        // calculate kernel coefficients
        for (int i = -kernel_size; i < kernel_size + 1; i++)
        {
            int i_adj = i + kernel_size;
            double numer = Math.Exp((-(i * i)) / (2 * sigma * sigma));
            double cell_value = numer / denom;
            kernel[i_adj] = cell_value;
            kernel_sum += cell_value;
        }

        // normalize kernel
        for (int i = 0; i < 2 * kernel_size + 1; i++)
        {
            kernel[i] /= kernel_sum;
            //Console.WriteLine(kernel[i]);
        }

        // apply kernel, with crop.
        for (int i = kernel_size; i < gesture.Count - kernel_size; i++)
        {
            Point_2D point = new Point_2D();
            point.x = 0.0f;
            point.y = 0.0f;
            point.time = 0.0f;
            for (int j = 0; j < 2 * kernel_size + 1; j++)
            {
                point.x += (float)kernel[j] * gesture[i - kernel_size + j].x;
                point.y += (float)kernel[j] * gesture[i - kernel_size + j].y;
                point.z += (float)kernel[j] * gesture[i - kernel_size + j].z;
                point.time += (float)kernel[j] * gesture[i - kernel_size + j].time;
            }
            output.Add(point);
        }

        return output;
    }

    /*
     * Returns the mean velocity vector for the gesture in world space.
     */
    private Point_3D mean_velocity_3D(ref List<Point_3D> gesture3d)
    {
        Point_3D v = new Point_3D();
        v.time = 0;
        v.x = 0;
        v.y = 0;
        v.z = 0;
        for (int i = 0; i < gesture3d.Count - 1; i++)
        {
            float dx = gesture3d[i + 1].x - gesture3d[i].x;
            float dy = gesture3d[i + 1].y - gesture3d[i].y;
            float dz = gesture3d[i + 1].z - gesture3d[i].z;
            float dt = gesture3d[i + 1].time - gesture3d[i].time;
            v.x += dx / dt;
            v.y += dy / dt;
            v.z += dz / dt;
        }

        v.x /= (gesture3d.Count - 1);
        v.y /= (gesture3d.Count - 1);
        v.z /= (gesture3d.Count - 1);

        return v;
    }

    /*
     * returns bounds of gesture for subsection described by start & end
     * parameters.
     */
    private Bounds calculate_bounds(
            ref List<Point_2D> gesture,
            int start, int end
    )
    {
        Bounds b = new Bounds();
        b.min_x = float.MaxValue;
        b.min_y = float.MaxValue;
        b.min_z = float.MaxValue;
        b.max_x = float.MinValue;
        b.max_y = float.MinValue;
        b.max_z = float.MinValue;

        for (int i = start; i < end; i++)
        {
            b.min_x = (gesture[i].x < b.min_x) ? gesture[i].x : b.min_x;
            b.min_y = (gesture[i].y < b.min_y) ? gesture[i].y : b.min_y;
            b.min_z = (gesture[i].z < b.min_z) ? gesture[i].z : b.min_z;
            b.max_x = (gesture[i].x > b.max_x) ? gesture[i].x : b.max_x;
            b.max_y = (gesture[i].y > b.max_y) ? gesture[i].y : b.max_y;
            b.max_z = (gesture[i].z > b.max_z) ? gesture[i].z : b.max_z;
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
            if (proximity_ab < PROXIMITY_THRESH ||
                    proximity_bc < PROXIMITY_THRESH)
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

    /*
     * Returns the dot product of two 2D vectors.
     */
    private float dot(Point_2D a, Point_2D b, bool ignore_z) {
        return a.x*b.x + a.y*b.y + ((ignore_z) ? 0 : a.z*b.z);
    }

    /*
     * Returns angle between two 2D vectors.
     */
    private float angle(Point_2D a, Point_2D b, bool ignore_z) {
        Point_2D na = normalize(a, ignore_z);
        Point_2D nb = normalize(b, ignore_z);
        return (float) Math.Acos(dot(na, nb, ignore_z));
    }

    public Gesture_Meta recognize_gesture(List<Point_2D> gesture, List<Point_3D> gesture3D) {
        Bounds bounds = calculate_bounds(ref gesture, 0, gesture.Count);
        List<float> angles = new List<float>();
        if (LOWPASS)
        {
            gesture = gaussian_average_lowpass(gesture, LP_KERNEL_SIZE, LP_SIGMA);
        }

        float angle_sum = calculate_angles(ref gesture, ref angles, bounds);
        Point_2D avg_vel_vec2 = mean_velocity_2D(ref gesture);
        Point_3D avg_vel_vec3 = mean_velocity_3D(ref gesture3D);

        float gesture_width = bounds.max_x - bounds.min_x;
        float gesture_height = bounds.max_y - bounds.min_y;
        Point_2D start = gesture[0];
        Point_2D end = gesture[gesture.Count - 1];

        Gesture_Meta ret = new Gesture_Meta();
        ret.type = Gesture.unknown;
        ret.avg_vel_vector = avg_vel_vec3;
        ret.angle_sum = angle_sum;
        ret.bounds = bounds;
 
        float vel_vec_norm = vec2_norm(avg_vel_vec2, false);
        float x_pos_dev = angle(avg_vel_vec2, new Point_2D(0,1,0,0),  false);
        float y_pos_dev = angle(avg_vel_vec2, new Point_2D(0,0,1,0),  false);
        float z_pos_dev = angle(avg_vel_vec2, new Point_2D(0,0,0,1),  false);
        float x_neg_dev = angle(avg_vel_vec2, new Point_2D(0,-1,0,0), false);
        float y_neg_dev = angle(avg_vel_vec2, new Point_2D(0,0,-1,0), false);
        float z_neg_dev = angle(avg_vel_vec2, new Point_2D(0,0,0,-1), false);
        bool x_pos = x_pos_dev < x_neg_dev;
        bool y_pos = y_pos_dev < y_neg_dev;
        bool z_pos = z_pos_dev < z_neg_dev;
        float gesture_width  = bounds.max_x - bounds.min_x;
        float gesture_height = bounds.max_y - bounds.min_y;
        float gesture_depth  = bounds.max_z - bounds.min_z;
        bool z_gesture = gesture_depth > gesture_height 
                && gesture_depth > gesture_width;

        //if(z_pos){
        //    z_gesture = (z_pos_dev < ((x_pos) ? x_pos_dev : x_neg_dev)) &&
        //            (z_pos_dev < ((y_pos) ? y_pos_dev : y_neg_dev));
        //} else {
        //    z_gesture = (z_neg_dev < ((x_pos) ? x_pos_dev : x_neg_dev)) &&
        //            (z_neg_dev < ((y_pos) ? y_pos_dev : y_neg_dev));        
        //}

        if(vel_vec_norm < VEL_VEC_NORM_THRESH && Math.Abs(angle_sum) > 6*ANGLE_SUM_THRESH) {
            // likely a circle (so far)
            float min_angle = (float) ((2 - ANGLE_SUM_THRESH) * Math.PI);
            float max_angle = (float) ((2 + ANGLE_SUM_THRESH) * Math.PI); 
            if (angle_sum > min_angle && angle_sum < max_angle) ret.type = Gesture.circle_ccw;
            if (angle_sum < -min_angle && angle_sum > -max_angle) ret.type = Gesture.circle_cw;
        } else if (vel_vec_norm > VEL_VEC_NORM_THRESH && z_gesture) {
            // likely something else 
            if     (z_pos_dev < ANGLE_DEVIATION_THRESH && Math.Abs(avg_vel_vec2.z) > 0.2) ret.type = Gesture.push;
            else if(z_neg_dev < ANGLE_DEVIATION_THRESH && Math.Abs(avg_vel_vec2.z) > 0.2) ret.type = Gesture.pull;
        } else if (vel_vec_norm > VEL_VEC_NORM_THRESH) {
            if     (x_pos_dev < ANGLE_DEVIATION_THRESH && Math.Abs(avg_vel_vec2.x) > 0.2) ret.type = Gesture.hline_lr;
            else if(x_neg_dev < ANGLE_DEVIATION_THRESH && Math.Abs(avg_vel_vec2.x) > 0.2) ret.type = Gesture.hline_rl;
            else if(y_pos_dev < ANGLE_DEVIATION_THRESH && Math.Abs(avg_vel_vec2.y) > 0.2) ret.type = Gesture.vline_du;
            else if(y_neg_dev < ANGLE_DEVIATION_THRESH && Math.Abs(avg_vel_vec2.y) > 0.2) ret.type = Gesture.vline_ud;

        }

        return ret;
    }
}

