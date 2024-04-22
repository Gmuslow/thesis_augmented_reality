import pandas as pd
import matplotlib.pyplot as plt
import numpy as np

df = pd.read_csv("csv2data.csv", header=None, index_col=0)
print(df)

def GetRow(match1, match2):
    filtered_rows = df[df.index.str.contains(match1) & df.index.str.contains(match2)]

    # If you want only one row (assuming there's only one matching row), you can use iloc
    desired_row = filtered_rows.iloc[0]  # Assuming there's only one matching row
    return desired_row

#for straight-on measurements
straightonRow1 = GetRow('straighton', '1 meter').to_numpy()
straightonRow2 = GetRow('straighton', '2 meter').to_numpy()
straightonRow3 = GetRow('straighton', '3 meter').to_numpy()
#straightonRow4 = GetRow('straighton', '4 meter').to_numpy()

#print(straightonRow1)

straightonRow1Coords = [(1, -int(i), (1, 0, 0)) for i in straightonRow1 if str(i) != "nan"]
straightonRow2Coords = [(2, -int(i), (1, 0, 0)) for i in straightonRow2 if str(i) != "nan"]
straightonRow3Coords = [(3, -int(i), (1, 0, 0)) for i in straightonRow3 if str(i) != "nan"]
#straightonRow4Coords = [(4, int(i), (1, 0, 0)) for i in straightonRow4 if str(i) != "nan"]
straightconcat = [*straightonRow1Coords, *straightonRow2Coords, *straightonRow3Coords]

#print(concat)

xS = [i[0] for i in straightconcat]
yS = [i[1] for i in straightconcat]
plt.scatter(xS, yS)
plt.title("Straight-On RSSI")
plt.xlabel("Distance (m)")
plt.ylabel("Recorded RSSI")
plt.show()

#for 90 degrees clockwise measurements

ninetyRow1 = GetRow('90 degrees', '1 meter').to_numpy()
ninetyRow2 = GetRow('90 degrees', '2 meter').to_numpy()
ninetyRow3 = GetRow('90 degrees', '3 meter').to_numpy()
#ninetyRow4 = GetRow('90 degrees', '4 meter').to_numpy()
#print all 
#print(straightonRow1)

ninetyRow1Coords = [(1, -int(i), (0, 1, 0)) for i in ninetyRow1 if str(i) != "nan"]
ninetyRow2Coords = [(2, -int(i), (0, 1, 0)) for i in ninetyRow2 if str(i) != "nan"]
ninetyRow3Coords = [(3, -int(i), (0, 1, 0)) for i in ninetyRow3 if str(i) != "nan"]
#ninetyRow4Coords = [(4, int(i), (0, 1, 0)) for i in ninetyRow4 if str(i) != "nan"]
ninetyconcat = [*ninetyRow1Coords, *ninetyRow2Coords, *ninetyRow3Coords]

#print(concat)

x90 = [i[0] for i in ninetyconcat]
y90 = [i[1] for i in ninetyconcat]
plt.scatter(x90, y90)
plt.title("90 Degree RSSI")
plt.xlabel("Distance (m)")
plt.ylabel("Recorded RSSI")
plt.show()

#180 degree RSSI

# oneEightyRow1 = GetRow('180 degrees', '1 meter').to_numpy()
# oneEightyRow2 = GetRow('180 degrees', '2 meter').to_numpy()
# oneEightyRow3 = GetRow('180 degrees', '3 meter').to_numpy()
# oneEightyRow4 = GetRow('180 degrees', '4 meter').to_numpy()
# #print all 
# #print(straightonRow1)

# oneEightyRow1Coords = [(1, int(i), (0, 0, 1)) for i in oneEightyRow1 if str(i) != "nan"]
# oneEightyRow2Coords = [(2, int(i), (0, 0, 1)) for i in oneEightyRow2 if str(i) != "nan"]
# oneEightyRow3Coords = [(3, int(i), (0, 0, 1)) for i in oneEightyRow3 if str(i) != "nan"]
# oneEightyRow4Coords = [(4, int(i), (0, 0, 1)) for i in oneEightyRow4 if str(i) != "nan"]
# oneeightyconcat = [*oneEightyRow1Coords, *oneEightyRow2Coords, *oneEightyRow3Coords, *oneEightyRow4Coords]

# #print(concat)

# x180 = [i[0] for i in oneeightyconcat]
# y180 = [i[1] for i in oneeightyconcat]
# plt.scatter(x180, y180)
# plt.title("180 Degree RSSI")
# plt.xlabel("Distance (m)")
# plt.ylabel("Recorded RSSI")
# plt.show()



#comparing antenna direction for each meter
oneMeters = [*[-int(i) for i in straightonRow1 if str(i) != "nan"]]#, *[int(i) for i in ninetyRow1 if str(i) != "nan"], *[int(i) for i in oneEightyRow1 if str(i) != "nan"]]
twoMeters = [*[-int(i) for i in straightonRow2 if str(i) != "nan"]]#, *[int(i) for i in ninetyRow2 if str(i) != "nan"], *[int(i) for i in oneEightyRow2 if str(i) != "nan"]]
threeMeters = [*[-int(i) for i in straightonRow3 if str(i) != "nan"]]#, *[int(i) for i in ninetyRow3 if str(i) != "nan"], *[int(i) for i in oneEightyRow3 if str(i) != "nan"]]
#fourMeters = [*[int(i) for i in straightonRow4 if str(i) != "nan"]]#, *[int(i) for i in ninetyRow4 if str(i) != "nan"], *[int(i) for i in oneEightyRow4 if str(i) != "nan"]]

oneMeterAvg = np.average(oneMeters)
twoMeterAvg = np.average(twoMeters)
threeMeterAvg = np.average(threeMeters)
#fourMeterAvg = np.average(fourMeters)

print("One Meter Average: " + str(oneMeterAvg))
print("Two Meter Average: " + str(twoMeterAvg))
print("Three Meter Average: " + str(threeMeterAvg))
#print("Four Meter Average: " + str(fourMeterAvg))

plt.scatter(xS, yS, color="red", label="Straight-On")
#plt.scatter(x180, y180, color="green", label="180 degree")
plt.scatter(x90, y90, color="blue", label="90 degree")
plt.scatter([1,2,3], [oneMeterAvg, twoMeterAvg, threeMeterAvg], color="pink", label="Averages")

plt.title("All Direction RSSI")
plt.xlabel("Distance (m)")
plt.ylabel("Recorded RSSI")
plt.legend()
plt.show()