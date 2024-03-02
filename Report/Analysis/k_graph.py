from find_k import compute_k
import csv

filename = "k_n.csv"
botData = [[] for _ in range(4)]

with open('Bot1Data.csv') as file:
    reader = csv.reader(file)
    for row in reader:
        botData[0].append(row)

with open('Bot2Data.csv') as file:
    reader = csv.reader(file)
    for row in reader:
        botData[1].append(row)

with open('Bot3Data.csv') as file:
    reader = csv.reader(file)
    for row in reader:
        botData[2].append(row)

with open('Bot4Data.csv') as file:
    reader = csv.reader(file)
    for row in reader:
        botData[3].append(row)

with open(filename, 'w') as file:
    writer = csv.writer(file)
    writer.writerow(['x', 'Bot 1', 'Bot 2', 'Bot 3', 'Bot 4'])

    for i in range(1, 26):
        k = [compute_k(bot, i) for bot in botData]
        writer.writerow([i] + k)
