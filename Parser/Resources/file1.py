#coding=utf-8
import json
import pickle
import pandas
import sys


VNActive =int(sys.argv[1])
OActive = int(sys.argv[2])
VBalance = int(sys.argv[3])
YK = int(sys.argv[4])
CapitalRez = int(sys.argv[5])
LongDebts = int(sys.argv[6])
ShortTDebts = int(sys.argv[7])
Revenue = int(sys.argv[8])
Profit = int(sys.argv[9])
Zaem = int(sys.argv[10])
CreditorDebt= int(sys.argv[11])
SalesProfit=int(sys.argv[12])
Bancrot = bool(sys.argv[13])
RNP = bool(sys.argv[14])
path = str(sys.argv[15])

df = pandas.DataFrame({'2020, Внеоборотные активы, RUB': [VNActive], '2020, Оборотные активы, RUB': [OActive],
                       '2020, Активы всего, RUB': [VBalance], '2020, Уставный капитал , RUB': [YK],
                       '2020, Капитал и резервы, RUB': [CapitalRez],
                       '2020, Долгосрочные обязательства, RUB': [LongDebts],
                       '2020, Краткосрочные обязательства, RUB': [ShortTDebts],
                       '2020, Выручка, RUB': [Revenue], '2020, Чистая прибыль (убыток), RUB': [Profit]})

with open(path, 'rb') as f:
    model_new = pickle.load(f)

prediction = model_new.predict(df)

try:
    AltmanIndex = -0.3877 - 1.0736 * (OActive / (Zaem + CreditorDebt)) + 0.0579 * (
                (LongDebts + ShortTDebts) / CapitalRez)
except ZeroDivisionError:
    AltmanIndex = None

try:
    TafflerIndex = 0.53 * (SalesProfit / ShortTDebts) + 0.13 * (OActive / (LongDebts + ShortTDebts)) + 0.18 * (
                ShortTDebts / VBalance) + 0.16 * (Revenue / VBalance)
except ZeroDivisionError:
    TafflerIndex = None

Result = 100
if TafflerIndex != None and AltmanIndex != None:
    if Bancrot:
        Result = 1
    elif RNP:
        Result = 1
    elif prediction == 1:
        Result = 1  # Высокий риск, обратите внимание на индексы, реестры и финансовые показатели
    elif df.values.any() == 0:
        Result = 1
    elif AltmanIndex > 0:
        Result = 1
    elif AltmanIndex == 0:
        Result = 1
    elif AltmanIndex < 0:
        Result = 2  # Риск низкий, сотрудничество возможно
    elif TafflerIndex < 0.2:
        Result = 1
    else:
        Result = 2
    print(Result)

if AltmanIndex == None and Result == 100:
    if Bancrot:
        Result = 1
    elif RNP:
        Result = 1
    elif prediction == 1:
        Result = 1  # Высокий риск, обратите внимание на индексы, реестры и финансовые показатели
    elif df.values.any() == 0:
        Result = 1
    elif TafflerIndex < 0.2:
        Result = 1
    else:
        Result = 2
    print(Result)

if TafflerIndex == None and Result == 100:
    if Bancrot:
        Result = 1
    elif RNP:
        Result = 1
    elif prediction == 1:
        Result = 1  # Высокий риск, обратите внимание на индексы, реестры и финансовые показатели
    elif df.values.any() == 0:
        Result = 1
    elif AltmanIndex > 0:
        Result = 1
    elif AltmanIndex == 0:
        Result = 1
    elif AltmanIndex < 0:
        Result = 2  # Риск низкий, сотрудничество возможно
    else:
        Result = 2
    print(Result)

if TafflerIndex == None and AltmanIndex == None and Result == 100:
    if Bancrot:
        Result = 1
    elif RNP:
        Result = 1
    elif prediction == 1:
        Result = 1  # Высокий риск, обратите внимание на индексы, реестры и финансовые показатели
    elif df.values.any() == 0:
        Result = 1
    else:
        Result = 2
    print(Result) #Риск низкий, сотрудничество возможно
