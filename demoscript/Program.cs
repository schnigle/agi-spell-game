using System;
using System.Collections.Generic;

namespace gestures
{
    class Program
    {

        /*
         * Constants
         */
        public const int ANGLE_GRANULARITY = 5;
        public const float PROXIMITY_THRESHOLD = 0.1f;
        public const int MIN_GESTURE_SIZE = 10;

        /*                                                                                           
         * a data point.                                                                             
         *                                                                                           
         * @param time timestamp since start of program (s)                                          
         * @param x x-coordinate on 2d plane                                                         
         * @param y y-coordinate on 2d plane
         * @param z depth value. 
         */
        struct point_2d 
        {
            public float time;
            public float x, y, z;
        }
        
        /*
         * Represents a bounding box.
         */
        struct bounds 
        {
            public float min_x, min_y, max_x, max_y;
        }

        /*
         * returns bounds of gesture for subsection described by start & end 
         * parameters.
         */
        private bounds calculate_bounds (
                ref List<point_2d> gesture, 
                int start, int end
        ) {
            bounds b = new bounds();
            b.min_x = float.MaxValue;
            b.min_y = float.MaxValue;
            b.max_x = float.MinValue;
            b.max_y = float.MinValue;

            for (int i = start; i < end; i++) {
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
            ref List<point_2d> gesture,
            ref List<float> angles,
            bounds bounds        
        ) {
            float angle_sum = 0; 
            angles.Add(0); // pad with 0
             
            for(int a = 0; a < gesture.Count - 2*ANGLE_GRANULARITY; a += ANGLE_GRANULARITY){
                int b = a + ANGLE_GRANULARITY;
                int c = a + 2*ANGLE_GRANULARITY;
                float vec_ab_x = gesture[b].x - gesture[a].x;
                float vec_ab_y = gesture[b].y - gesture[a].y;
                float vec_bc_x = gesture[c].x - gesture[b].x;
                float vec_bc_y = gesture[c].y - gesture[b].y;
                
                // calculate angle.
                float signed_angle = (float) (Math.Atan2(vec_bc_y, vec_bc_x) - 
                        Math.Atan2(vec_ab_y, vec_ab_x));
                
                // if angle > 180 then angle := 360 - angle
                if (signed_angle > Math.PI)
                    signed_angle -= (float) (2*Math.PI);
                if (signed_angle < -Math.PI)
                    signed_angle += (float) (2*Math.PI);

                // check proximity. If points are too close then skip.
                float proximity_ab = proximity(gesture[a], gesture[b], bounds);
                float proximity_bc = proximity(gesture[b], gesture[c], bounds);      
                if(proximity_ab < PROXIMITY_THRESHOLD || 
                        proximity_bc < PROXIMITY_THRESHOLD) {
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
        float proximity(point_2d a, point_2d b, bounds bounds) 
        {
            float gesture_width = bounds.max_x - bounds.min_x;
            float gesture_height = bounds.max_y - bounds.min_y;
            float relative_dx = (b.x - a.x) / gesture_width;
            float relative_dy = (b.y - a.y) / gesture_height;
            float relative_distance = (float) Math.Sqrt(relative_dx*relative_dx 
                    + relative_dy*relative_dy);
            return relative_distance; 
        }

        public Program() {
            List<point_2d> gesture = new List<point_2d>();
            List<float> angles = new List<float>();
        
            string line;
            while ((line = Console.ReadLine()) != null && line != "") {
                string[] value_str = line.Split(' ');
                var data_point = new point_2d();
                data_point.time = float.Parse(value_str[0]);
                data_point.x = float.Parse(value_str[1]);
                data_point.y = float.Parse(value_str[2]);
                data_point.z = float.Parse(value_str[3]);
                gesture.Add(data_point);
                //Console.WriteLine(line);
            }
            
            // bounding box for entire gesture
            bounds b = calculate_bounds(ref gesture, 0, gesture.Count);
            float angle_sum = calculate_angles(ref gesture, ref angles, b);

            // super simple (and ugly) ad hoc classifier
            float gesture_width = b.max_x - b.min_x;
            float gesture_height = b.max_y - b.min_y;
            point_2d start = gesture[0];
            point_2d end = gesture[gesture.Count - 1];
            
            // I'm sorry
            if(angle_sum > 1.75*Math.PI && angle_sum < 2.25*Math.PI) {
                Console.WriteLine("counter-clockwise circle");
            } else if (angle_sum < -1.75*Math.PI && angle_sum > -2.25*Math.PI) {
                Console.WriteLine("clockwise circle");
            } else if (Math.Abs(angle_sum) < 0.25 && gesture_width > 4*gesture_height
                    && gesture_width > 0.15) {
                if(start.x > end.x) {
                    Console.WriteLine("right to left line");
                } else {
                    Console.WriteLine("left to right line");
                } 
            } else if (Math.Abs(angle_sum) < 0.25 && gesture_height > 4*gesture_width
                    && gesture_height > 0.15) {
                if(start.y > end.y) {
                    Console.WriteLine("downward line");
                } else {
                    Console.WriteLine("upward line");
                }
            } else {
                Console.WriteLine("UNKNOWN GESTURE");
            }


            //Console.Write("\n");
            //
            //for (int i = 0; i < gesture.Count; i++) {
            //    Console.Write(gesture[i].time + " ");
            //    Console.Write(gesture[i].x + " ");
            //    Console.Write(gesture[i].y + " ");
            //    Console.Write(gesture[i].z);
            //    Console.Write("\n");
            //}
        }    

        static void Main(string[] args)
        {
            new Program();
        }
    }
}
