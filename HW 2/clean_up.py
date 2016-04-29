import re

inputFile = open('enwiki-20160407-all-titles-in-ns0')
outputFile = open('cleanedNames', 'w+')
for line in inputFile:
    linestripped = line.strip()
    clean = re.sub(r'([^\_a-zA-Z])+', '', line)
    if linestripped == clean:
        outputFile.write(line)

outputFile.flush()
outputFile.close()
