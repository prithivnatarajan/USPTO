"""

This script does not run on its own but is used in conjuction 
with SummarizeData.py

It takes a ./HTML downloaded using DownloadData.py
and returns all the required information in the following order:

[patID, grantDate, title, abstract, citations, CPC, inventor, assignee, claims, examiner, patClass, description]

This can be changed by appropriately changing the variable 'output'

"""

"""

########################  ATTENTION:  ########################
All assumptions about the file format were made by closely analysing
the structure of the HTML files. These files were downloaded from PatFT.

		http://patft.uspto.gov/netahtml/PTO/index.html

"""

# Python library to read HTML files easily.
from bs4 import BeautifulSoup

def Scrape(htmlPage: BeautifulSoup) -> [str]:
	patID = ''
	grantDate = ''
	inventor = ''
	assignee = ''
	CPC = ''
	title = ''
	abstract = ''
	claims = ''
	citations = ''
	examiner = ''
	patClass = ''
	description = ''
	readingClaims = False
	readingClass = False
	readingDescription = False

	data = htmlPage.find_all(text = True)

	for i in range(len(data) - 1, -1, -1):
		data[i] = str(data[i]).strip()
		if (data[i] == ''):
			del data[i]

	for i in range(len(data)):
		if data[i] == "United States Patent":
			patID = data[i + 1].replace('/', '').replace(',', '')

		elif data[i] == "Abstract":
			abstract = data[i + 1]
			title = data[i - 1]
			if (data[i-2] == '**'):
				if (data[i-5][0] == '*'):
					grantDate = data[i - 6]
				else:
					grantDate = data[i - 5]
			else:
				grantDate = data[i - 2]

		elif data[i] == "Assignee:":
			assignee = data[i + 1]

		elif data[i] == "Inventors:":
			j = i+1
			while (data[j] != "Applicant:" and data[j] != "Assignee:" and data[j] != "Family ID:"):
				inventor += data[j]
				j = j+1
			inventor = inventor.replace('; ', ' | ')

		elif data[i] == "Current CPC Class:":
			readingClass = False
			CPC = data[i + 1]
			start = CPC.find('(')
			while (start != -1):
				end = CPC.find(')')
				CPC = CPC[:start-1] + CPC[end+1:]
				start = CPC.find('(')
			CPC = CPC.replace(' ', '').replace(';', ' | ')

		elif data[i] == "U.S. Patent Documents":
			j = i+1
			while (data[j][0].isdigit()):
				citations += data[j].replace('/', '').replace(',', '') + ' | '
				j = j+3

		elif data[i] == "Claims":
			readingClaims = True

		elif data[i] == "Description":
			readingDescription = True
			readingClaims = False

		elif data[i] == "Primary Examiner:":
			examiner = data[i+1].replace('; ', ' | ')
		
		elif data[i] == 'Current U.S. Class:':
			readingClass = True

		elif data[i] == 'Current International Class:':
			readingClass = False

		elif data[i] == '* * * * *':
			readingDescription = False

		elif readingClass:
			patClass += data[i]

		elif readingClaims:
			claims += data[i]

		elif readingDescription:
			description += ' ' + data[i]

	patClass = patClass.replace(' ', '').replace(';', ' | ')

	output = [patID, grantDate, title, abstract, citations, CPC, inventor, assignee, claims, examiner, patClass, description]
	return output
