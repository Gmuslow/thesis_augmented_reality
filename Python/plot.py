import numpy as np
import matplotlib.pyplot as plt
import random
import math

def plot_idw(points):
    """points come in the form (numSensors, numVertices, executionTime(ms))"""
    X = np.array([i[0] for i in points])
    Y = np.array([i[1] for i in points])
    Z = np.array([i[2] for i in points])

    fig = plt.figure(figsize=(10, 8))
    ax = fig.add_subplot(111, projection='3d')

    # Scatter plot for the first 5 points
    ax.scatter(X[:6], Y[:6], Z[:6], c='royalblue', marker='o', s=50)

    # Scatter plot for the second 5 points
    ax.scatter(X[6:12], Y[6:12], Z[6:12], c='orange', marker='o', s=50)

    # Scatter plot for the second 5 points
    ax.scatter(X[12:], Y[12:], Z[12:], c='red', marker='o', s=50)

    # Connect the first 5 points with a line
    ax.plot(X[:6], Y[:6], Z[:6], c='royalblue', linestyle='-', linewidth=2)

    # Connect the second 5 points with a line
    ax.plot(X[6:12], Y[6:12], Z[6:12], c='orange', linestyle='-', linewidth=2)

    # Connect the second 5 points with a line
    ax.plot(X[12:], Y[12:], Z[12:], c='red', linestyle='-', linewidth=2)

    ax.set_xlabel('Number of Sensors')
    ax.set_ylabel('Number of Vertices')
    ax.set_zlabel('Execution Time (ms)')

    plt.show()

def Link_Budget():
    arr = np.linspace(1, 15, 1000) #x values
    EIRP = 5 #effective isotropic radiated power
    antenna_array_loss = 10 #bad
    sigma = 5 #5dB normal for standard deviation
    n = 4  #environmental factor
    PL_0 = 40 #precalculated loss at 1 meter
    avg_vals = []
    withAntennaLoss = []

    losses = []
    for i in range(len(arr)):
        val =  PL_0 + 10 * n * math.log10(arr[i]) 
        avg_vals.append(EIRP - val - antenna_array_loss)
        val += random.gauss(0, sigma)
        losses.append(EIRP - val - antenna_array_loss)

    plt.plot([0, 1], [EIRP, EIRP - PL_0 - antenna_array_loss], c="orange", label="EIRP")
    plt.plot(arr, losses)
    plt.plot(arr, avg_vals, color="yellow")
    plt.axhline(y = -88, color = 'r', linestyle = '--') 
    plt.axvline(x = 5, color = "green", linestyle = "--")
    plt.ylabel('dBm')
    plt.xlabel('Distance (m)')
    plt.legend(["EIRP to after loss at 1 meter", "Rx power (gauss)", "Rx power (mean)", "Rx sensitivity (-88dBm)", "Target Range"])
    plt.show()

if __name__ == "__main__":
    points = [(5, 512, 0), (5, 1024, 1), (5, 2048, 2), (5, 4096, 4), (5, 8192, 9), (5, 16384, 18), 
              (10, 512, 1), (10, 1024, 2), (10, 2048, 4), (10, 4096, 8), (10, 8192, 17), (10, 16384, 35), 
              (15, 512, 1), (15, 1024, 3), (15, 2048, 6), (15, 4096, 13), (15, 8192, 26), (15, 16384, 52)]
    #plot_idw(points)
    Link_Budget()
    
