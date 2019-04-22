import collections
import configparser
import os
import re
import sqlite3
from random import randint

import itertools
import numpy
import pandas as pd
import time
import numpy as np
import datetime
from dateutil import parser
import json

i = input('subclass number\n')
os.system('copy ..\\..\\data\\SubClassInfo\\patent_info' + i + '.tsv ..\\..\\data\\patent_info.tsv')

def citations_str_to_cumulative_dict(s: pd.Series):
    """
    :param s: Series where first element is a patent id, second element is citation date
    :return: Dictionary keyed by citation date, values are cumulative citations up to that date
    """
    if s.isnull().any():
        return dict()

    patent_ids = s[0]
    grant_date = pd.to_datetime(s[1])
    citations_series = pd.DataFrame(
        [f'{randint(grant_date.year, 2018)}-{randint(grant_date.month, 12)}-{randint(min(grant_date.day, 28),28)}' for y
         in
         patent_ids.split('|')],
        columns=['date'])
    citations_series['date'] = pd.to_datetime(citations_series['date'])
    return citations_series.groupby(by='date').size().sort_index().cumsum().to_dict()


def get_patent_info_df(file_path):
    df = pd.read_csv(file_path, encoding='utf8', dialect='excel-tab',
                     delimiter='\t',
                     names=['patentId',
                            'grantDate',
                            'title',
                            'abstract',
                            'citations',
                            'cpc',
                            'inventor',
                            'assignee',
                            'claims', 'examiners','patClass'], index_col=False)

    df.set_index('patentId', inplace=True)
    df['assignee'] = df['assignee'].apply(clean_assignee_string)
    return df


# remove uneccessary words from assignee names to make them less messy
kill_words = {'corp', 'co', 'ltd', 'inc', 'gmbh', 'llc', 'sa', 'corporation', 'lp', 'company', 'kft'}

# make the stop assignees better looking; long assignee names are messy
# this should probably be done for the most common assignees
manual_assignee_disambiguation = {'Thales Alenia Space Italia SpA': 'Thales',
                                  'Hewlett Packard Development': 'HP',
                                  'ザ ボーイング カンパニーＴｈｅ Ｂｏｅｉｎｇ Ｃｏｍｐａｎｙ': 'Boeing',
                                  'Massachusetts Institute Of Technology': 'MIT',
                                  'Massachusetts Institute of Technology': 'MIT',
                                  'Quanergy Systems': 'Quanergy',
                                  'Northeastern University Boston': 'Northeastern Univ',
                                  'E I du Pont de Nemours and': 'DuPont',
                                  'Thomas Swan and': 'Thomas Swan',
                                  'Korea Research Institute of Standards and Science': 'KRISS',
                                  'Troyes Universite de Technologie de': 'Univ of Tech of Troyes',
                                  'Leland Stanford Junior University': 'Stanford Univ',
                                  'University of Southern California USC': 'Univ of Southern Cali',
                                  'Samsung Electronics': 'Samsung',
                                  'National Aeronautics and Space Administration NASA': 'NASA',
                                  'GM Global Technology Operations': 'GM',
                                  'Electronics and Telecommunications Research Institute': 'ETRI',
                                  'LIGHTLAB IMAGING': 'Lightlab Imaging',
                                  'Max Planck Gesellschaft zur Forderung der Wissenschaften': 'Max Planck Society',
                                  'US Secretary of Navy': 'US Navy'
                                  }
word_regex = re.compile(r'\w+', re.IGNORECASE)


def clean_assignee_string(s: str) -> str:
    """
    Removes words like "corp", "ltd" and "llc" from the end of each assignee
    :param s: input string of assignees, separated by " | "
    :return: cleaned assignee string
    """
    output_words = []
    assignees = [x.strip() for x in s.split(' | ')]
    for assignee in assignees:
        input_words = word_regex.findall(assignee)
        for i, word in enumerate(reversed(input_words)):
            if word.lower() in kill_words:
                continue
            elif i == 0:
                output_words.append(' '.join(input_words))
            else:
                output_words.append(' '.join(input_words[:-i]))
            break

    # manually disambiguate
    output_words = [manual_assignee_disambiguation[a] if a in manual_assignee_disambiguation else a for a in
                    output_words]

    # remove duplicates, keeping order
    output_words = list(collections.OrderedDict.fromkeys(output_words))
    return ' | '.join(output_words)


def get_patent_3d_info(file_path: str, patent_spread_multiplier: float) -> pd.DataFrame:
    df = pd.read_csv(file_path, encoding='ISO-8859-1')
    df.rename(columns={'patnum': 'patentId',
                       'x_coor': 'x',
                       'y_coor': 'y',
                       'z_coor': 'z'},
              inplace=True)
    df.set_index('patentId', inplace=True)
    df[['x', 'y', 'z']] *= patent_spread_multiplier
    df = df[['x', 'y', 'z', 'examiners']]
    df.dropna(inplace=True)
    assert not df.isnull().values.any()
    return df


def date_to_unix_timestamp(d: str):
	return int(time.mktime(datetime.datetime.strptime(d, "%B %d, %Y").timetuple()))
    #return int(time.mktime(parser.parse(d).timetuple()))


def unix_timestamp_to_date(u: int):
    return str(datetime.datetime.utcfromtimestamp(u).date())


def int_series_to_normalized_float_series(s: pd.Series) -> pd.Series:
    min_timestamp = s.min()
    max_timestamp = s.max()
    return (s - min_timestamp) / (max_timestamp - min_timestamp)


def get_complete_patent_df(df1: pd.DataFrame, df2: pd.DataFrame, time_spread=1.0) -> pd.DataFrame:
    combined_df = df1.join(df2, how='outer', lsuffix='_left')
    combined_df.drop(columns= ['examiners_left'])
    combined_df.dropna(inplace = True)
    combined_df['patentId'] = combined_df.index
    combined_df['shortCpc'] = combined_df['cpc'].apply(lambda x: x.strip()[0])
    combined_df['textFields'] = combined_df.to_dict(orient='records')

    # TODO: must update once citations are correct
    # (currently contains related patents, forward citations, and backward citations)
    combined_df['citedByFiltered'] = combined_df[['patentId', 'citations']].apply(
        lambda x: pd.Series([y.strip() for y in x[1].split('|') if
                             y.strip() in combined_df.index and y.strip() != x[0]]).unique().tolist(), axis=1)
    combined_df['citations'] = combined_df[['citations', 'grantDate']].apply(citations_str_to_cumulative_dict, axis=1)

    combined_df['assignee'] = combined_df['assignee'].apply(lambda x: [y.strip() for y in x.split('|')][0])

    combined_df['examiners'] = combined_df['examiners'].apply(lambda x: [y.strip() for y in x.split(';')][0])

    combined_df['grantDateUnix'] = combined_df['grantDate'].apply(date_to_unix_timestamp)
    print(datetime.datetime.utcfromtimestamp(combined_df['grantDateUnix'].min()).strftime('%B %d, %Y'))
    print(datetime.datetime.utcfromtimestamp(combined_df['grantDateUnix'].max()).strftime('%B %d, %Y'))
    print(len(combined_df['patentId']))
    
    combined_df['t'] = int_series_to_normalized_float_series(combined_df['grantDateUnix']) * time_spread

    return combined_df


def get_cluster_df(combined_df):
    mean_xyz_df = combined_df[['cluster', 'x', 'y', 'z']].groupby('cluster').mean()
    cluster_term_df = combined_df[['cluster', 'terms']].drop_duplicates().set_index('cluster')
    cluster_term_df = mean_xyz_df.join(how='inner', other=cluster_term_df)
    cluster_term_df['cluster'] = cluster_term_df.index
    return cluster_term_df


def run():
    os.chdir('../../../')  # set directory to root so paths in config.ini make sense

    config = configparser.ConfigParser()
    config.read('Assets/Scripts/DataManagement/config.ini')
    config = config['PatentInfoGenerator']
    timeline_spread = float(config['TimelineSpread'])
    num_timeline_labels = int(config['NumTimelineLabels'])

    df1 = get_patent_info_df(config['PatentInfoLocation'])

    df2 = get_patent_3d_info(config['PatentClusterInfoLocation'], float(config['PatentSpreadMultiplier']))
    combined_df = get_complete_patent_df(df1, df2, time_spread=timeline_spread)

    if config.getboolean('TimelineEnabled'):
        combined_df['z'] = combined_df['t']

    combined_df[['patentId', 'grantDate', 'x', 'y', 'z', 'citations', 'citedByFiltered', 'textFields', 'examiners']].to_json(
        'assets/data/patent.json', orient='records')

    # cluster_term_df = get_cluster_df(combined_df)
    # cluster_term_df.to_json('assets/data/cluster_terms.json', orient='records')

    timeline_label_positions = np.linspace(0, timeline_spread, num_timeline_labels).tolist()
    timeline_labels = pd.Series(
        np.linspace(combined_df['grantDateUnix'].min(), combined_df['grantDateUnix'].max(), num_timeline_labels)).apply(
        lambda x: str(datetime.datetime.utcfromtimestamp(x).date())).tolist()
    with open(config['TimelineLabelsLocation'], 'w') as f:
        json.dump(
            dict(labels=timeline_labels, positions=timeline_label_positions),
            f)


def analyze_assignees():
    """
    Use/modify this for debugging purposes. It allows SQL queries to be made on the patent data.
    :return:
    """
    os.chdir('../../')  # set directory to root so paths in config.ini make sense

    config = configparser.ConfigParser()
    config.read('assets/config.ini')
    config = config['PatentInfoGenerator']

    df1 = get_patent_info_df(config['PatentInfoLocation'])

    df2 = get_patent_3d_info(config['PatentClusterInfoLocation'], float(config['PatentSpreadMultiplier']))
    combined_df = get_complete_patent_df(df1, df2)

    w = re.compile(r'\w+', re.IGNORECASE)
    q = (w.findall(x) for x in df1['assignee'].values)
    words = list(itertools.chain.from_iterable(q))
    word_series = pd.DataFrame(words, columns=['word'])
    with sqlite3.connect(':memory:') as db:
        db.row_factory = sqlite3.Row
        cur = db.cursor()
        word_series.to_sql('word', db, index=False)
        combined_df[['grantDate']].to_sql('patent', db, index=False)
        cur.execute(
            'select count(*) from patent where cluster = "4" and grantDate >= "2018-01-01" and grantDate < "2018-04-01"')
        print([dict(x) for x in cur.fetchall()])


if __name__ == '__main__':
    run()
