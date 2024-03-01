import csv
import sys

# pass as a command line argument the percentage threshold X, to compute K_N(X)
# for X = 1%, run:
# python find_k.py 1
X = 1 if not sys.argv else float(sys.argv[1])

# must be true for all m > N, not just m = len(data)
def compute_k(data):
    for N in range(1, len(data) - 1):
        for m in range(N + 1, len(data)):
            total = sum(float(row[1]) for row in data[N + 1: m + 1])
            average = total / (m - N)
        
            if average >= X:
                break
        else:
            return N
    
    return None

bot1Data = []
with open('Bot1Data.csv') as file:
    reader = csv.reader(file)
    for row in reader:
        bot1Data.append(row)
    bot1K = compute_k(bot1Data)
    print("Bot 1 K:", bot1K)

bot2Data = []
with open('Bot2Data.csv') as file:
    reader = csv.reader(file)
    for row in reader:
        bot2Data.append(row)
    bot2K = compute_k(bot2Data)
    print("Bot 2 K:", bot2K)

bot3Data = []
with open('Bot3Data.csv') as file:
    reader = csv.reader(file)
    for row in reader:
        bot3Data.append(row)
    bot3K = compute_k(bot3Data)
    print("Bot 3 K:", bot3K)

bot4Data = []
with open('Bot4Data.csv') as file:
    reader = csv.reader(file)
    for row in reader:
        bot4Data.append(row)
    bot4K = compute_k(bot4Data)
    print("Bot 4 K:", bot4K)
