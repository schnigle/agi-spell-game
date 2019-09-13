#define PI 3.1415926535
#define ANGLE_GRANULARITY 5
#define PROXIMITY_THRESHOLD 0.1
#define MIN_GESTURE_SIZE 10

#include <iostream>
#include <string>
#include <sstream>
#include <vector>
#include <limits>
#include <math.h>

/*
 * a data point.
 *
 * @param time timestamp since start of program (s)
 * @param x x-coordinate on 2d plane 
 * @param y y-coordinate on 2d plane
 * @param z depth value. 
 */
struct point_2d {
    float time;
    float x, y, z;
};

/*
 * Represents a bounding box.
 */
struct bounds {
    float min_x, min_y, max_x, max_y;
};

/*
 * returns bounds of gesture for subsection described by start & end 
 * parameters.
 */
bounds calculate_bounds(
        const std::vector<point_2d> &gesture,
        size_t start,
        size_t end
) {
    bounds b;
    b.min_x = std::numeric_limits<float>::max();
    b.min_y = std::numeric_limits<float>::max();
    b.max_x = std::numeric_limits<float>::min();
    b.max_y = std::numeric_limits<float>::min();
    
    for(size_t i = start; i < end; i++) {
        b.min_x = (gesture[i].x < b.min_x) ? gesture[i].x : b.min_x; 
        b.min_y = (gesture[i].y < b.min_y) ? gesture[i].y : b.min_y; 
        b.max_x = (gesture[i].x > b.max_x) ? gesture[i].x : b.max_x; 
        b.max_y = (gesture[i].y > b.max_y) ? gesture[i].y : b.max_y; 
    }

    return b;
}

/*
 * Returns distance between two points relative to the size of the gesture.
 */
float proximity(const point_2d &a, const point_2d &b, const bounds &bounds) {
    float gesture_width = bounds.max_x - bounds.min_x;
    float gesture_height = bounds.max_y - bounds.min_y;
    float relative_dx = (b.x - a.x) / gesture_width;
    float relative_dy = (b.y - a.y) / gesture_height;
    float relative_distance = 
            sqrt(relative_dx*relative_dx + relative_dy*relative_dy);
    return relative_distance;
}

/*
 * Calculates the angles between a gestures data points, and stores them in
 * @angles. Returns angle sum for subsection described by start & end 
 * parameters. @angles should be empty.
 *
 * An angle is the relative angle between two lines AB and BC described by 
 * three points A, B and C. In other words, if A, B and C are all points on a
 * single line the angle will be 0. A gesture of n points will have n-2 such
 * angles, as the first and last points only have one connecting edge.
 */
float calculate_angles(
        const std::vector<point_2d> &gesture, 
        std::vector<float> &angles,
        const bounds &bounds
) {
    float angle_sum = 0; 
    angles.push_back(0); // pad with 0
     
    for(size_t a = 0; a < gesture.size() - 2*ANGLE_GRANULARITY; a += ANGLE_GRANULARITY){
        int b = a + ANGLE_GRANULARITY;
        int c = a + 2*ANGLE_GRANULARITY;
        float vec_ab_x = gesture[b].x - gesture[a].x;
        float vec_ab_y = gesture[b].y - gesture[a].y;
        float vec_bc_x = gesture[c].x - gesture[b].x;
        float vec_bc_y = gesture[c].y - gesture[b].y;
        //float vec_ab_len = sqrt(vec_ab_x * vec_ab_x + vec_ab_y * vec_ab_y);
        //float vec_bc_len = sqrt(vec_bc_x * vec_bc_x + vec_bc_y * vec_bc_y);
        
        //cos(angle)
        //float cos_angle = (vec_ab_x*vec_bc_x + vec_ab_y*vec_bc_y) 
        //        / (vec_ab_len*vec_bc_len);
        //float angle = acos(cos_angle);

        float signed_angle = atan2(vec_bc_y, vec_bc_x) - atan2(vec_ab_y, vec_ab_x);
        
        if (signed_angle > PI)
            signed_angle -= 2*PI;
        if (signed_angle < -PI)
            signed_angle += 2*PI;

        if(
                proximity(gesture[a], gesture[b], bounds) < PROXIMITY_THRESHOLD || 
                proximity(gesture[b], gesture[c], bounds) < PROXIMITY_THRESHOLD
        ) {
            continue;    
        }
    
        angles.push_back(signed_angle);
        angle_sum += signed_angle;
        //std::cout << std::endl;
        //std::cout << gesture[a].x << " " << gesture[a].y << std::endl;
        //std::cout << gesture[b].x << " " << gesture[b].y << std::endl;
        //std::cout << gesture[c].x << " " << gesture[c].y << std::endl;
        //std::cout << "angle: " << signed_angle << std::endl;
        //std::cout << "ab proximity: " << proximity(gesture[a], gesture[b], bounds) << std::endl;
        //std::cout << "bc proximity: " << proximity(gesture[b], gesture[c], bounds) << std::endl;
        //std::cout << "ac proximity: " << proximity(gesture[a], gesture[c], bounds) << std::endl;
         
        //std::cout << gesture[b].x << " " << gesture[b].y << " " << signed_angle << std::endl;
    }

    return angle_sum;
}

int main(int argc, char *argv[]) {
    // data 
    std::vector<point_2d> gesture; 
    std::vector<float> angles;

    // read input
    std::string line;
    std::string value_str;
    std::stringstream ss;
    while(getline(std::cin, line, '\n')) {
        float time, x, y, z;
        ss << line;
        getline(ss, value_str, ' ');
        time = std::stof(value_str);    
        getline(ss, value_str, ' ');
        x = std::stof(value_str);
        getline(ss, value_str, ' ');
        y = std::stof(value_str);
        getline(ss, value_str, ' ');
        z = std::stof(value_str);
        gesture.push_back({time, x, y, z});
        ss.clear();
    }

    if(gesture.size() < MIN_GESTURE_SIZE){
        std::cout << "Gesture too short" << std::endl;
        exit(0);
    }

    bounds b = calculate_bounds(gesture, 0, gesture.size()); 

    float angle_sum = calculate_angles(gesture, angles, b); 
    
    std::cout << "angle sum: " << angle_sum << std::endl;

    // super simple (and ugly) ad hoc classifier
    float gesture_width = b.max_x - b.min_x;
    float gesture_height = b.max_y - b.min_y;
    point_2d start = gesture[0];
    point_2d end = gesture[gesture.size() - 1];
    
    // I'm sorry
    if(angle_sum > 1.75*PI && angle_sum < 2.25*PI) {
        std::cout << "counter-clockwise circle" << std::endl;
    } else if (angle_sum < -1.75*PI && angle_sum > -2.25*PI) {
        std::cout << "clockwise circle" << std::endl;
    } else if (fabs(angle_sum) < 0.25 && gesture_width > 4*gesture_height
            && gesture_width > 0.15) {
        if(start.x > end.x) {
            std::cout << "right to left line" << std::endl;
        } else {
            std::cout << "left to right line" << std::endl;
        } 
    } else if (fabs(angle_sum) < 0.25 && gesture_height > 4*gesture_width
            && gesture_height > 0.15) {
        if(start.y > end.y) {
            std::cout << "downward line" << std::endl;
        } else {
            std::cout << "upward line" << std::endl;
        }
    } else {
        std::cout << "unknown gesture" << std::endl;
    }

}
