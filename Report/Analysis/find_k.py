import csv


def compute_k(data):
    i = len(data) - 1
    count = 0
    total = 0

    while count == 0 or total/count <= 1:
        total += float(data[i][1])
        count += 1
        i -= 1

    return i


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
