import sys
import pandas as pd
import math
import json

usage = '''

==================================== USAGE: =====================================
This scripts takes a tab delimited input file, checks for formating
and creates VRdata.json file to be used in the Unity VR setup.

Usage: python converData.py input.tsv

================================== PARAMETERS: ==================================
The input tab delimited file may contain the following parameters

Mandatory:   Label, x, y, z
Optional:    Date, Filter, Info, Color, Size, Transparency

If an optional parameter is provided for one data point it must be provided 
for all. Otherwise, that column will be ignored for all data points. Similarly
for the filters, each data point must contain a value for the given filter.
If any data point does not contain a value then a warning will be shown.

==================================== FORMAT: ====================================
Label:  String
x:      Float
y:      Float
z:      Float

Date:   YYYY-MM-DD

Filters are used to filter the data points in the VR Space
Filter: "Filter1: value | Filter2: value"
        Eg. "Assignee: Microsoft | CPC: G | Inventor: Saurabh Parikh | Examiner: Prithiv Natarajan"

Info is shown on the data point info panel. The heading is the Label and the text
shown is formated as follows. Info1, Info2, Info3... are bold the text under them
is of a regular font.
Info:   "Info1: value | Info2: value"
        Eg. "Title: This is a patent | Abstract: Its about VR | Claims: Its awesome"

Color:  Hex value
        Eg. ff00ff

Size:   Float (0,1]

Transparency: Float (0, 1]

=================================================================================


'''

# Checks to look for missing or incorrectly formated data in each column
def Run(file_path):
    df = pd.read_csv(file_path, delimiter='\t')

    numPoints = len(df['Label'])
    numFilters = 0
    filters = {}
    
    # x, y, z
    for i in range(numPoints):
        try:
            if (math.isnan(float(df['x'][i]))):
                raise Exception('\n\n ******* x coordinate for ' + str(df['Label'][i]) + ' is in the incorrect format. ******* \n')
            if (math.isnan(float(df['y'][i]))):
                raise Exception('\n\n ******* y coordinate for ' + str(df['Label'][i]) + ' is in the incorrect format. ******* \n')
            if (math.isnan(float(df['z'][i]))):
                raise Exception('\n\n ******* z coordinate for ' + str(df['Label'][i]) + ' is in the incorrect format. ******* \n')
        except:
            print('EXCEPTION: Check if x, y, z values are correctly formated for ' + str(df['Label'][i]))
            print('           Given: x: ' + str(df['x'][i]) + ' y: ' + str(df['y'][i]) + ' z: ' + str(df['z'][i]))
            print('           Expected: Float')
            exit()

    # Date
    if ('Date' in df.columns):
        for i in range(numPoints):
            try:
                year,month,day = df['Date'][i].split('-')
                if (len(year) != 4 or len(month) != 2 or len(day) != 2):
                    raise
                if (int(month) > 12 or int(month) < 1 or int(day) > 31 or int(day) < 1):
                    raise
            except:
                print('WARNING: Date for ' + str(df['Label'][i]) + ' is in the incorrect format. Ignoring all values.')
                print('         Given: ' + str(df['Date'][i]))
                print('         Expected: YYYY-MM-DD')
                print('WARNING: Time based sorting will not work in VR.')
                df.drop('Date', axis = 1, inplace = True)
                break

    # Filter
    if ('Filter' in df.columns):
        
        # Finding all filter values
        keyVals = df['Filter'][0].split(' | ')
        for kv in keyVals:
            key = kv[:kv.find(':')]
            filters[key] = []

        # Filling all filter values and removing filter column if any filter does not exist
        for i in range(numPoints):
            try:
                keyVals = df['Filter'][i].split(' | ')
                if (len(keyVals) != len(filters)):
                    raise

                for kv in keyVals:
                    key = kv[:kv.find(':')]
                    value = kv[kv.find(':') + 2:]
                    filters[key].append(value)
            except:
                print('WARNING: Filters for ' + str(df['Label'][i]) + ' do not match. Ignoring all filters.')
                print('         Given: ' + str(df['Filter'][i]))
                print('         Expected Filters: ', end = '')
                for f in filters:
                    print(f, end = ', ')
                print('')
                filters = {}
                df.drop('Filter', axis = 1, inplace = True)
                break

        # Add new filter columns to the data frame
        if (len(filters) != 0):
            df.drop('Filter', axis = 1, inplace = True)
            for k, v in filters.items():
                df['Filter: ' + k] = v

'''
======================================================================================================
                                            TO BE IMPLEMENTED
======================================================================================================

    # Info
    if ('Info' in df.columns):
        for i in range(numPoints):
            try:
                pass
            except:
                print('WARNING: Size for ' + str(df['Label'][i]) + ' is in the incorrect format. Ignoring all values.')
                print('         Given: ' + str(df['Info'][i]))
                print('         Expected: "Info1: value | Info2: value"')
                df.drop('Info', axis = 1, inplace = True)
                break

======================================================================================================
                                                END
======================================================================================================

'''
    # Color
    if ('Color' in df.columns):
        for i in range(numPoints):
            try:
                hexCol = df['Color'][i]
                if (str(len(hexCol)) != 6):
                    raise
                r, g, b = int(hexCol[:2], 16), int(hexCol[2:4], 16), int(hexCol[4:6], 16)
                if (r > 255 or r < 0 or g > 255 or g < 0 or b > 255 or b < 0):
                    raise        
            except:
                print('WARNING: Color for ' + str(df['Label'][i]) + ' is in the incorrect format. Ignoring all values.')
                print('         Given: ' + str(df['Color'][i]))
                print('         Expected: hex color code, like \'ff00ff\'')
                df.drop('Color', axis = 1, inplace = True)
                break

    # Size
    if ('Size' in df.columns):
        for i in range(numPoints):
            try:
                if (float(df['Size']) > 1 or float(df['Size']) <= 0):
                    raise
            except:
                print('WARNING: Size for ' + str(df['Label'][i]) + ' is in the incorrect format. Ignoring all values.')
                print('         Given: ' + str(df['Size'][i]))
                print('         Expected: (0, 1]')
                df.drop('Size', axis = 1, inplace = True)
                break
    
    # Transparency
    if ('Transparency' in df.columns):
        for i in range(numPoints):
            try:
                if (float(df['Transparency']) > 1 or float(df['Transparency']) <= 0):
                    raise
            except:
                print('WARNING: Transparency for ' + str(df['Label'][i]) + ' is in the incorrect format. Ignoring all values.')
                print('         Given: ' + str(df['Transparency'][i]))
                print('         Expected: (0, 1]')
                df.drop('Transparency', axis = 1, inplace = True)
                break

    df.to_json('./VRdata.json', orient='records')


if __name__ == '__main__':
    if (len(sys.argv) == 2):
            Run(sys.argv[1])
    else:
        print("ERROR: Incorrect number of parameters. \n" + usage) 

