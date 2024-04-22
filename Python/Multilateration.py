import matplotlib.pyplot as plt
import argparse
import math
import numpy as np
from typing import List
from matplotlib.patches import Circle
from mpl_toolkits.mplot3d import Axes3D

"""The idea is, the larger the circle, the less likely it is to be accurate since
the signals are more likely to be noisy, resulting in inaccurate RSSI values.
A larger priority results in the coordinate more likely to be highlighted."""
def PriorityFunction(rssi) -> float:
    return math.sqrt((abs(rssi) - 40) )
def sigmoid(x, shift):
    return 1 / (1 + np.exp(-(x - shift)))
def GetColors(values) -> List[float]:
    maxval = max(values)
    threshold = np.average(values)
    print(f"Max Val: {maxval}")
    #values = [0 if val > threshold else 1 for val in values]
    return [(1 - value / maxval, 1 - value / maxval, 1 - value / maxval) for value in values]

coord_filename = "current_coord.txt"
current_coordinates = [] #the 2D current coordinate of the user.
rssi_values = []

"""Extract coordinate, RSSI from file"""
with open(coord_filename, "r") as infile:
    lines = infile.read().split("$")[:-1]
    
    for line in lines:
        line = line.replace("{", "").replace("}", "").strip()
        #print(line.split(";"))
        coord, RSSI = line.split(";")
        RSSI = float(RSSI)
        print(coord)
        coord = coord.replace("(", "").replace(")", "").strip()
        x = coord.split(",")[0]
        y = coord.split(",")[1]
        z = coord.split(",")[2]
        current_coordinates.append([float(x), float(y), float(z)])
        rssi_values.append(RSSI)
        
print(f"Current Coordinates: {current_coordinates}")
print(f"Current RSSI values: {rssi_values}")

"""Translate RSSI into meters to get radius of circle."""
radii = []
for val in rssi_values:
    N = 4 #environmental factor
    measured_power = -40 #measured power at 1m
    exp = (measured_power - val) / (10 * N)
    radius = math.pow(10, exp)
    radii.append(radius)
print(f"Circle Radii: {radii}")

"""Create Starting Grid and loop through each point to calculate 
distance from circles"""
maxDist = 5 #max distance in the x and y directions (10, 10) in meters
maxPoints = 21 #how many points will be between (-maxDist, maxDist)
candidate_points = []
possible_x = np.linspace(-maxDist, maxDist, maxPoints)
possible_y = np.linspace(-maxDist, maxDist, maxPoints)
possible_z = np.linspace(-maxDist, maxDist, maxPoints)

for x in possible_x:
    for y in possible_y:
        for z in possible_z:
            candidate_points.append([x, y, z])
#print(f"Candidate Points: {candidate_points}")



"""Loop through all points"""
candidate_values = []
for candidate in candidate_points:
    candidate_value = 0.0
    combined_distance_score = 0.0
    for i in range(len(current_coordinates)):
        vector = [candidate[0] - current_coordinates[i][0], candidate[1] - current_coordinates[i][1], candidate[2] - current_coordinates[i][2]]
        mag = math.sqrt(math.pow(vector[0], 2) + math.pow(vector[1], 2) + math.pow(vector[2], 2)) #distance formula
        distance = abs(mag - radii[i]) #distance = distance from candidate point to the edge of the current circle.
        combined_distance_score += distance * PriorityFunction(rssi_values[i])
        #candidate_value += PriorityFunction(rssi_values[i]) / distance
        #print(f"Candidate {candidate} has distance {distance} from circle {current_coordinates[i]}. NewVal: {candidate_value}")
    candidate_value = 1.0 / combined_distance_score
    candidate_values.append(candidate_value)
#print(f"Candidate Values: {candidate_values}")

colors = GetColors(candidate_values)
show2DGraph = False

if show2DGraph:
    xcoords = [p[0] for p in candidate_points]
    ycoords = [p[1] for p in candidate_points]
    
    plt.scatter(xcoords, ycoords, c=colors)

    x_center = [c[0] for c in current_coordinates]
    y_center = [c[1] for c in current_coordinates]

    #2D
    for i in range(len(current_coordinates)):
        circle_center = (x_center[i], y_center[i])  # Define the center of the circle
        circle_radius = radii[i]  # Define the radius of the circle
        circle = Circle(circle_center, circle_radius, edgecolor='r', facecolor='none')
        plt.gca().add_patch(circle)
    plt.axis("equal")
    plt.grid(True)
    plt.xlabel("X direction")
    plt.ylabel("Y direction")
    plt.show()

show3DGraph = True
if show3DGraph:
    fig = plt.figure(figsize=(13,10))
    ax = fig.add_subplot(111, projection='3d')
    xcoords = [p[0] for p in candidate_points]
    ycoords = [p[1] for p in candidate_points]
    zcoords = [p[2] for p in candidate_points]

    x_center = [c[0] for c in current_coordinates]
    y_center = [c[1] for c in current_coordinates]
    z_center = [c[2] for c in current_coordinates]
    ax = fig.add_subplot(111, projection='3d')
    
    
    ax.scatter(xcoords, ycoords, zcoords, alpha=0.4, c=colors, s=2)
    for i in range(len(current_coordinates)):
        radius = radii[i]
        resolution = 100
        u = np.linspace(0, 2 * np.pi, resolution)
        v = np.linspace(0, np.pi, resolution)
        x_sphere = radius * np.outer(np.cos(u), np.sin(v)) + current_coordinates[i][0]
        y_sphere = radius * np.outer(np.sin(u), np.sin(v)) + current_coordinates[i][1]
        z_sphere = radius * np.outer(np.ones(np.size(u)), np.cos(v)) + current_coordinates[i][2]
        ax.plot_surface(x_sphere, y_sphere, z_sphere, color='b', alpha=0.1)  # Set alpha to control transparency
    ax.set_xlabel('X')
    ax.set_ylabel('Y')
    ax.set_zlabel('Z')
    plt.show()


with open("candidate_data.txt", "w") as outfile:
    for i in range(len(candidate_points)):
        outfile.write(f"{candidate_points[i]},{candidate_values[i]}\n")

"""
Test Data
{(-1.54893, -3.02); -50}
{(-2.54893, 3.02); -50}

"""