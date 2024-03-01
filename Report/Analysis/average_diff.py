import csv
import sys

# gives the average difference between the two data sets over the entire range
# note: the result is negative if data1 is greater than data2, and positive if data2 is greater than data1
def average_diff(data1, data2): 
    total = 0
    minLen = min(len(data1), len(data2))
    for i in range(1, minLen):
        if data1[i][2] == "NaN" or data2[i][2] == "NaN":
            continue
        total += float(data2[i][2]) - float(data1[i][2])
    return total / minLen

if not sys.argv:
    print("No data given")
    sys.exit(1)

file1 = sys.argv[1]
file2 = sys.argv[2]

data1 = []
with open(file1) as file:
    reader = csv.reader(file)
    for row in reader:
        data1.append(row)

data2 = []
with open(file2) as file:
    reader = csv.reader(file)
    for row in reader:
        data2.append(row)

print(average_diff(data1, data2))