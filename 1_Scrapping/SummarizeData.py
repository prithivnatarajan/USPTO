'''

This script uses ScapeData.py and combines all the 
data downloaded using DownloadData.py into one file
patent_info.tsv. This file will be used to carry out
the data analysis.

'''

# Python libraries
import csv
from bs4 import BeautifulSoup

# Custom script
from ScrapeData import Scrape

numPatents = 13539

with open('patent_info.tsv', 'w+', newline = '') as tsvfile:
	writer = csv.writer(tsvfile, delimiter = '\t')

	header = [	"patID", 
				"grantDate", 
				"title",
				"abstract",
				"citations",
				"CPC",
				"inventor",
				"assignee",
				"claims",
				"examiner",
				"patClass",
				"description"]

	writer.writerow(header)

	for i in range(1, numPatents):
		with open('./ScrappedData/pat' + str(i) + '.html') as file:
			data = Scrape(BeautifulSoup(file.read(), 'html.parser'))
			writer.writerow(data)
