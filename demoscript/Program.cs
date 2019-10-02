using System;
using System.Collections.Generic;

namespace gestures
{
    class Program
    {

        /*
         * Constants
         */
        public const int ANGLE_GRANULARITY = 10;            // - How many nodes we skip per angle calculation.
                                                            // keep high (ca 5-10 works good) unless when using 
                                                            // with lowpass filter.
        public const float PROXIMITY_THRESH = 0.2f;         // - Minimum required distance between nodes for
                                                            // calculating angle.
        public const int MIN_GESTURE_SIZE = 30;             // - Minimum number of nodes in a gesture. Shorter nodes
                                                            // return unknown.
        public const float ANGLE_SUM_THRESH = 0.5f;         // - Used when determining circle type gestures. A circle
                                                            // is accepted when angle_sum ∈ [2*pi - angle_sum_thresh,
                                                            // 2*pi + angle_sum_thres].
        public const float VEL_VEC_NORM_THRESH = 0.25f;     // - |vel_vec| < vel_vec_norm_thresh counts as zero
                                                            // internally.
        public const float ANGLE_DEVIATION_THRESH = 0.5f;   // - Used for linear gesture segments. Angle deviation
                                                            // describes difference in angle between two vectors.
                                                            // Angles > angle_deviation_thresh rules out a 
                                                            // candidate gesture
        public const float Z_DEPTH_THRESH = 0.3f;           // If depth of gesture is larger than this, consider
                                                            // gestures with depth as candidates.

        // Constants for segmentation. 
        public const float SEGMENT_MIN_EUQLIDIAN_DISTANCE = 0.0f; // - Segments are ignored if the euqlidian distance
                                                                  // between the first and last point are less than
                                                                  // this.
        public const int SPARSIFY_GRANULARITY = 3;                // - How many nodes we skip in for each iteration in
                                                                  // the sparsify function.
        public const float SEGMENTATION_ANGLE_THRESH = 0.5f;      // - Threshold for directional devitation used when
                                                                  // segmenting a gesture.
        public const int SEGMENTATION_LENGTH_THRESH =             // - Segments with fewer # of points than this will
                10 / SPARSIFY_GRANULARITY;                        // be ignored.

        // Determines how sensitive we want to be in regards to thresholds
        // on an axis. 0.5f means doubling the thresholds.
        public const float X_GESTURE_THRESH_MULTIPLIER = 1f;
        public const float Y_GESTURE_THRESH_MULTIPLIER = 1f;
        public const float Z_GESTURE_THRESH_MULTIPLIER = 0.5f;

        // Constants for lowpass filter.
        public const bool LP_ENABLE = true;
        public const int LP_KERNEL_SIZE = 3;
        public const float LP_SIGMA = 4.0f;

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
            square_cw,
            eqlat_triangle_cw,
            spiral_cw,
            zigzag_z,
            diamond_cw,
            sigma,
            fish_r,
            unknown
        };

        /*
         * Map from segmented gesture length (# of segments) to 
         * list of possible gestures.
         */
        private Dictionary<int, Gesture[]> gesture_len_map = 
                new Dictionary<int, Gesture[]> {
            [1] = new Gesture[] {Gesture.hline_lr, Gesture.hline_rl, Gesture.vline_du, Gesture.vline_ud},        
            [3] = new Gesture[] {Gesture.eqlat_triangle_cw, Gesture.zigzag_z, Gesture.fish_r},        
            [4] = new Gesture[] {Gesture.square_cw, Gesture.diamond_cw, Gesture.sigma}        
        };
  
        /*
         * Map from Gesture type to a boolean value stating whether alternative
         * starting points are allowed.
         */ 
        private Dictionary<Gesture, bool> alt_start_allowed_map = 
                new Dictionary<Gesture, bool> {
            [Gesture.hline_lr] = false,
            [Gesture.hline_rl] = false,
            [Gesture.vline_ud] = false,
            [Gesture.vline_du] = false,
            [Gesture.eqlat_triangle_cw] = true,
            [Gesture.zigzag_z] = false,
            [Gesture.fish_r] = false,
            [Gesture.square_cw] = true,
            [Gesture.diamond_cw] = true,
            [Gesture.sigma] = false
        };

        // Hard coded linear segment compositions for gestures. 
        private Dictionary<Gesture, float[]> reference_segments = 
                new Dictionary<Gesture, float[]>
        {
            [Gesture.hline_lr] = new float[] {0.0f},
            [Gesture.hline_rl] = new float[] {(float)Math.PI},
            [Gesture.vline_ud] = new float[] {(float)(1.5f*Math.PI)},
            [Gesture.vline_du] = new float[] {(float)(0.5f*Math.PI)},
            [Gesture.square_cw] = new float[] {
                    0.0f, 
                    (float)(1.5f*Math.PI), 
                    (float)(Math.PI), 
                    (float)(0.5f*Math.PI) 
            },
            [Gesture.eqlat_triangle_cw] = new float[] {
                    (float)((5.0f/3.0f)*Math.PI), 
                    (float)(Math.PI), 
                    (float)((1.0f/3.0f)*Math.PI)
            },
            [Gesture.zigzag_z] = new float[] {
                    0.0f, 
                    (float)((5.0f/4.0f)*Math.PI), 
                    0.0f
            },
            [Gesture.diamond_cw] = new float[] {
                    (float)((7.0f/4.0f)*Math.PI),
                    (float)((5.0f/4.0f)*Math.PI),
                    (float)((3.0f/4.0f)*Math.PI),
                    (float)((1.0f/4.0f)*Math.PI),
            },
            [Gesture.sigma] = new float[] {
                    (float)(Math.PI),
                    (float)((7.0f/4.0f)*Math.PI),
                    (float)((5.0f/4.0f)*Math.PI),
                    0.0f,
            },
            [Gesture.fish_r] = new float[] {
                    (float)((7.0f/4.0f)*Math.PI),
                    (float)(0.5f*Math.PI),
                    (float)((5.0f/4.0f)*Math.PI)
            }
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

        /*
         * Struct describing a line segment.
         *
         * @len number of points in the segment
         * @start_idx index of first point in segment
         * @end_idx index of last point in segment
         * @val associated value - for our purposes an angle describing the direction
         */
        private struct LinearSegment
        {
            public int len;
            public int start_idx, end_idx;
            public float val;
            public LinearSegment(int len, int start_idx, int end_idx, float val) {
                this.len = len;
                this.start_idx = start_idx;
                this.end_idx = end_idx;
                this.val = val; 
            }
        }

        /*
         * Returns the normalized vector of a 2D vector.
         */
        private Point_2D normalize(Point_2D vec2, bool ignore_z) {
            Point_2D ret;
            float norm = vec2_norm(vec2, ignore_z);
            ret.time = vec2.time;
            ret.x = vec2.x / norm;
            ret.y = vec2.y / norm;
            ret.z = vec2.z / ((ignore_z) ? 1 : norm);
            return ret; 
        }
        
        /*
         * returns the norm of a 3D vector.
         */
        private float vec3_norm(Point_3D vec)
        {
            return (float)Math.Sqrt(vec.x * vec.x + vec.y * vec.y + vec.z * vec.z);
        }

        /*
         * returns the norm of a 2D vector.
         */
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
         * Calculates forward derivative for a list of vectors (Point_2D).
         * Discards last vector.
         */
        private List<Point_2D> derive_vector_list(ref List<Point_2D> list) {
            List<Point_2D> d_list = new List<Point_2D>();
            for(int i = 0; i < list.Count - 1; i++) {
                float dx = list[i+1].x - list[i].x;
                float dy = list[i+1].y - list[i].y;
                float dz = list[i+1].z - list[i].z;
                float dt = list[i+1].time - list[i].time;
                d_list.Add(new Point_2D(list[i].time, dx/dt, dy/dt, dz/dt));
            }
            return d_list;
        }

        /*
         * Calculates norms for a list of vectors (Point_2D).
         */
        private float[] vector_list_norms(ref List<Point_2D> list, bool ignore_z) {
            float[] norms = new float[list.Count];
            for(int i = 0; i < norms.Length; i++) {
                float norm = (float) Math.Sqrt(
                        list[i].x * list[i].x +
                        list[i].y * list[i].y +
                        ((ignore_z) ? 0 : list[i].z * list[i].z)
                );
                norms[i] = norm;
            }
            return norms;
        }

        /*
         * Ignores some gesture points based on SPARSIFY_GRANULARITY.
         */
        private List<Point_2D> sparsify(ref List<Point_2D> gesture) {
            List<Point_2D> sparse_gesture = new List<Point_2D>();
            for(int i = 0; i < gesture.Count; i += SPARSIFY_GRANULARITY) {
                sparse_gesture.Add(gesture[i]);
            }
            return sparse_gesture;
        }

        /*
         * Returns an array of angles describing the direction vectors
         * of the first n - 1 points of a gesture with n points.
         */
        private float[] vector_list_angles(ref List<Point_2D> gesture) {
            float[] angles = new float[gesture.Count - 1];
            Point_2D e1 = new Point_2D(0, 1, 0, 0);
            for(int i = 0; i < angles.Length; i++) {
                Point_2D a = gesture[i];
                Point_2D b = gesture[i+1];
                float dx = b.x - a.x;
                float dy = b.y - a.y;
                angles[i] = (float) ((Math.Atan2(dy, dx) + 2*Math.PI) % (2*Math.PI));
            }
           return angles; 
        }

        /*
         * Extracts linear features from an array of angles. The angles 
         * correspond to the direction vectors of the first n - 1 points
         * of a gesture with n points.
         */
        private List<LinearSegment> find_linear_segments(float[] angles) {
            List<LinearSegment> segments = new List<LinearSegment>();
            int current_seg = 0;
            float current_angle_sum = 0;
            int current_len = 0;
            int current_start_idx = 0;

            for (int i = 0; i < angles.Length; i++) {
                float angle = angles[i];
                float current_avg = current_angle_sum / current_len;
                
                // adjust if angle is close to 2*PI    
                if(angle - current_avg > Math.PI){
                    angle -= (float) (2*Math.PI);
                } else if (angle - current_avg < -Math.PI) {
                    angle += (float) (2*Math.PI);
                }

                float deviation = Math.Abs(angle - current_avg);
                if (deviation > SEGMENTATION_ANGLE_THRESH || i == angles.Length - 1) {
                    if(current_len > SEGMENTATION_LENGTH_THRESH){
                        current_avg += (float) (2*Math.PI);
                        current_avg %= (float) (2*Math.PI);
                        current_seg++;
                        segments.Add(
                            new LinearSegment(
                                current_len,        // len
                                current_start_idx,  // start_idx
                                i - 1,              // end_idx
                                current_avg         // val
                            )
                        );
                    }
                    current_len = 1;
                    current_angle_sum = angle;
                    current_start_idx = i;
                } else {
                    current_angle_sum += angle;
                    current_len++;
                } 
            }

            return segments;
        }

        /*
         * Returns true if the segmented gesture described by @segs matches
         * the reference segments of the gesture described by @gesture_label.
         */
        private bool matches_linear_segments(
                List<LinearSegment> segs,
                Gesture gesture_label,
                List<Point_2D> gesture,
                bool allow_alt_start
        ) {
            float[] reference = reference_segments[gesture_label];
            int j = 0; // to iterate through reference.
            int k = 0; // number of times we've offset the start.

            
            //if (reference.Length > segs.Count)
            if (reference.Length != segs.Count)
                return false;
        
            for (int i = 0; i < segs.Count; i++) {
                float x0 = gesture[segs[i].start_idx].x;
                float y0 = gesture[segs[i].start_idx].y;
                float x1 = gesture[segs[i].end_idx].x;
                float y1 = gesture[segs[i].end_idx].y;
                float dx = x1 - x0;
                float dy = y1 - y0;
                float euq_len = (float) Math.Sqrt(dx*dx + dy*dy);
                if (euq_len < SEGMENT_MIN_EUQLIDIAN_DISTANCE)
                   continue;


                // adjust if angle is close to 2*PI.
                float val = segs[i].val;
                if (val - reference[j] > Math.PI) {
                    val -= (float)(2*Math.PI); 
                } else if (val - reference[j] < -Math.PI) {
                    val += (float)(2*Math.PI); 
                }

                // compare to reference.
                if (Math.Abs(val - reference[j]) > ANGLE_DEVIATION_THRESH) {
                    if(!allow_alt_start || k > reference.Length)
                        return false;
                    k++;
                    i = -1;
                }
                
                // segment accepted. Proceed to next reference segment.
                j++;
                j %= reference.Length;
            }

            // segments accepted.
            return true;
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

        public Program() {
            List<Point_2D> gesture = new List<Point_2D>();
        
            string line;
            while ((line = Console.ReadLine()) != null && line != "") {
                string[] value_str = line.Split(' ');
                var data_point = new Point_2D();
                data_point.time = float.Parse(value_str[0]);
                data_point.x = float.Parse(value_str[1]);
                data_point.y = float.Parse(value_str[2]);
                data_point.z = float.Parse(value_str[3]);
                gesture.Add(data_point);
            }
       
            // above is for demo code only
            Console.WriteLine("start");
            var watch = System.Diagnostics.Stopwatch.StartNew();
                 
            Bounds bounds = calculate_bounds(ref gesture, 0, gesture.Count);
            List<float> angles = new List<float>();
            if (LP_ENABLE)
            {
                gesture = gaussian_average_lowpass(gesture, LP_KERNEL_SIZE, LP_SIGMA);
            }

            float angle_sum = calculate_angles(ref gesture, ref angles, bounds);
            Point_2D avg_vel_vec2 = mean_velocity_2D(ref gesture);
            //Point_3D avg_vel_vec3 = mean_velocity_3D(ref gesture3D);

            float gesture_width  = bounds.max_x - bounds.min_x;
            float gesture_height = bounds.max_y - bounds.min_y;
            float gesture_depth  = bounds.max_z - bounds.min_z;
            Point_2D start = gesture[0];
            Point_2D end = gesture[gesture.Count - 1];

            Gesture_Meta ret = new Gesture_Meta();
            ret.type = Gesture.unknown;
            //ret.avg_vel_vector = avg_vel_vec3;
            ret.angle_sum = angle_sum;
            ret.bounds = bounds;

            // get velocity & acceleration vectors for gestures
            List<Point_2D> sparse_gesture = sparsify(ref gesture);
            List<Point_2D> vel_vectors = derive_vector_list(ref sparse_gesture);
            List<Point_2D> acc_vectors = derive_vector_list(ref vel_vectors);
            float[] vel_norms = vector_list_norms(ref vel_vectors, true);
            float[] acc_norms = vector_list_norms(ref acc_vectors, true);
            float[] global_angles = vector_list_angles(ref sparse_gesture);
 
            // ******************** gesture classification *********************
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
            bool z_gesture = gesture_depth > gesture_height 
                    && gesture_depth > gesture_width;

            if(vel_vec_norm < VEL_VEC_NORM_THRESH && Math.Abs(angle_sum) > 6*ANGLE_SUM_THRESH) {
                // likely a circle
                float min_angle = (float) ((2 - ANGLE_SUM_THRESH) * Math.PI);
                float max_angle = (float) ((2 + ANGLE_SUM_THRESH) * Math.PI); 
                if (angle_sum > min_angle && angle_sum < max_angle) ret.type = Gesture.circle_ccw;
                if (angle_sum < -min_angle && angle_sum > -max_angle) ret.type = Gesture.circle_cw;
            } else if (vel_vec_norm > VEL_VEC_NORM_THRESH && z_gesture || gesture_depth > Z_DEPTH_THRESH) {
                // likely a gesture with depth 
                if     (z_pos_dev < ANGLE_DEVIATION_THRESH && Math.Abs(avg_vel_vec2.z) > 0.2) ret.type = Gesture.push;
                else if(z_neg_dev < ANGLE_DEVIATION_THRESH && Math.Abs(avg_vel_vec2.z) > 0.2) ret.type = Gesture.pull;
            } else if (ret.type == Gesture.unknown) {
                // likely a gesture of one or more line segments
                List<LinearSegment> segments = find_linear_segments(global_angles);
                Console.WriteLine(segments.Count);
                Gesture[] candidate_gestures = gesture_len_map[segments.Count]; 
                foreach (Gesture g in candidate_gestures) {
                    if (matches_linear_segments(segments, g, sparse_gesture, 
                                alt_start_allowed_map[g])){
                        ret.type = g;
                        break;
                    }
                }
            }

            // Debug print
            watch.Stop();
            float time = watch.ElapsedMilliseconds;
            Console.WriteLine(time + "ms");
            Console.WriteLine(ret.type);
           
        }    

        static void Main(string[] args)
        {
            new Program();
        }
    }
}
