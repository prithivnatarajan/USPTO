"""

This script scrapes the data from the PatFT website for USPC 706
class. The data is stored locally for further analysis.
The process can be stopped anytime by pressing CTRL + C

"""

# Python libraries
import requests
import time
import random
import os

# number of patents in USPC 706
numPatents = 13539

for i in range(1, numPatents + 1):
	fname = './ScrappedData/pat' + str(i) + '.html' # Name of the file where the data will be stored

	# If the file exists, continue. 
	# The getsize method as sometimes the server returns empty files.
	# We want to download such files again.
	if (os.path.isfile(fname)):
		if os.path.getsize(fname) > 10000:
			continue

	# A counter to see progress.
	print('PATENT' + str(i))

	# Change this link accordingly.
	link = 'http://patft.uspto.gov/netacgi/nph-Parser?Sect1=PTO2&Sect2=HITOFF&u=%2Fnetahtml%2FPTO%2Fsearch-adv.htm&r=' + str(i) + '&p=1&f=G&l=50&d=PTXT&S1=706%2F$.CCLS.&OS=ccl/706/$'

	# Setting User-Agent tells the server who is asking for the file.
	# Here we emulate Google Chrome.
	# An empty User-Agent may cause a connection error.
	headers = {'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3626.121 Safari/537.36'}

	# Keep trying until the server returns the request.
	with open(fname, 'w+', newline = '') as file:
		while True:
			try:
				response = requests.get(link, headers=headers)

				# Arbitrary check to see if the file was empty or not. Check created by looking at what empty files look like
				if (response.text.count('\n') < 50):
					print('Missing Data: Try Again')
					raise Exception('Missing Data: Try Again')
				else:
					file.write(response.text)
			except KeyboardInterrupt: # CTRL + C
				print("\n Stopping Process")
				exit()
			except: # Server didn't respond.
				print("Failed connection: Trying again PATENT" + str(i))
				time.sleep(random.uniform(5, 8))
				continue
			break # Move to next file.