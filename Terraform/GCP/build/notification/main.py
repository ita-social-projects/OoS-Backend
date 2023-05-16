from __future__ import annotations

import base64

from datetime import datetime

import functions_framework

import json

import os

import requests

from typing import TypedDict, List, Optional

FRONT_REPO = "ita-social-projects/OoS-Frontend"
BACK_REPO = "ita-social-projects/OoS-Backend"


@functions_framework.cloud_event
def subscribe(cloud_event) -> None:
  webhook = os.getenv('WEBHOOK_URL')
  if (webhook is None):
    print("Webhook url not set.")
    return

  build: GoogleCloudBuild = json.loads(
      base64.b64decode(cloud_event.data["message"]["data"]))
  if "deploy" not in build['substitutions']['TRIGGER_NAME']:
    print('Not deploy job.')
    return

  status = ['WORKING', 'SUCCESS', 'FAILURE', 'INTERNAL_ERROR', 'TIMEOUT']

  if build['status'] in status:
    msg = create_message(build)
    print(send_discord_message(webhook, msg))
  else:
    print("Required status not found")


def create_message(build: GoogleCloudBuild) -> DiscordMessage:
  embeds: List[Embed] = []
  tag = build['substitutions']['_IMAGE_TAG']
  commit = tag.split(':')[-1]
  url = None

  if "front" in tag:
    content = "UI application"
    url = f"https://{build['substitutions']['_HOST']}"
    if build['status'] == 'WORKING':
      commit_info = get_github_commit(FRONT_REPO, commit)
  elif "api" in tag:
    content = "Web API"
    url = f"https://{build['substitutions']['_HOST']}/swagger/index.html"
    if build['status'] == 'WORKING':
      commit_info = get_github_commit(BACK_REPO, commit)
  elif "auth" in tag:
    content = "Auth server"
    commit_info = None
  if build['status'] == 'WORKING':
    embeds.append({
        'color':
        1127128,
        'title':
        "🔨 DEPLOYING",
        'description':
        f"Deployment started at {datetime.fromisoformat(build['startTime']).strftime('%A %-d %-Y, %H:%M:%S')}",
    })
    if commit_info is not None:
      embeds.append({
          'color': 1127128,
          'title': commit,
          'description': commit_info['commit']['message'],
          'url': commit_info['html_url'],
          'author': {
              'name': commit_info['author']['login'],
              'icon_url': commit_info['author']['avatar_url']
          }
      })
  elif build['status'] == 'SUCCESS':
    embeds.append({
        'color':
        1027128,
        'title':
        "✅ SUCCESS",
        'description':
        f"Deployment took a {(datetime.fromisoformat(build['finishTime']) - datetime.fromisoformat(build['startTime'])).seconds} seconds."
    })
    if "auth" not in tag:
      embeds.append({
          'color': 1027128,
          'title': url,
          'description': "Click to test new features.",
          'url': url
      })
  else:
    embeds.append({
        'color': 14177041,
        'title': f"❌ ERROR - {build['status']}",
        'description': "Click to see the log.",
        'url': build['logUrl']
    })
  return {'content': content, 'embeds': embeds}


def send_discord_message(webhook: str, msg: DiscordMessage) -> str:
  r = requests.post(webhook, json=msg)
  if r.status_code == requests.codes.ok:
    return "Success!"
  else:
    return r.text


def get_github_commit(repo: str, commit_sha: str) -> GithubCommitInfo | None:
  r = requests.get(f"https://api.github.com/repos/{repo}/commits/{commit_sha}")
  if r.status_code == requests.codes.ok:
    return r.json()
  else:
    return None


class GoogleCloudBuild(TypedDict):
  id: str
  status: str
  steps: Optional[List[Step]]
  createTime: str
  startTime: str
  finishTime: str
  substitutions: RequiredSubstitutions
  logUrl: str


class RequiredSubstitutions(TypedDict):
  _HOST: str
  _IMAGE_TAG: str
  TRIGGER_NAME: str


class Step(TypedDict):
  name: str
  args: List[str]
  entrypoint: str
  timing: Timing
  pullTiming: Timing
  status: str


class Timing(TypedDict):
  startTime: str
  endTime: str


class DiscordMessage(TypedDict):
  content: str
  embeds: List[Embed]


class Embed(TypedDict):
  title: str
  color: int
  description: Optional[str]
  url: Optional[str]
  author: Optional[EmbedAuthor]


class EmbedAuthor(TypedDict):
  name: str
  icon_url: str


class GithubCommitInfo(TypedDict):
  sha: str
  commit: GithubCommit
  author: GithubCommitAuthor
  html_url: str


class GithubCommit(TypedDict):
  author: str
  message: str


class GithubCommitAuthor(TypedDict):
  login: str
  avatar_url: str
