from __future__ import annotations

import base64

from datetime import datetime

import functions_framework

import json

import os

import requests

from typing import TypedDict, List, Optional


@functions_framework.cloud_event
def subscribe(cloud_event) -> None:
    webhook = os.getenv('WEBHOOK_URL')
    if (webhook is None):
      print("Webhook url not set.")
      return

    build: GoogleCloudBuild = json.loads(base64.b64decode(cloud_event.data["message"]["data"]))
    if "deploy" not in build['substitutions']['TRIGGER_NAME']:
      print('Not deploy job.')
      return

    status = ['WORKING', 'SUCCESS', 'FAILURE', 'INTERNAL_ERROR', 'TIMEOUT']

    if build['status'] in status:
      msg = createMessage(build)
      print(sendDiscordMessage(webhook, msg))
    else:
      print("Required status not found")

def createMessage(build: GoogleCloudBuild) -> DiscordMessage:
  embeds: List[Embed] = []
  tag = build['substitutions']['_IMAGE_TAG']
  commit = tag.split(':')[-1]
  template = "{} with commit id **{}**"
  url = None

  if "front" in tag:
    content = template.format("UI application", commit)
    url = f"https://{build['substitutions']['_HOST']}"
    commit_url = f"https://github.com/ita-social-projects/OoS-Frontend/commit/{commit}"
  elif "api" in tag:
    content = template.format("Web api", commit)
    url = f"https://{build['substitutions']['_HOST']}/swagger/index.html"
    commit_url = f"https://github.com/ita-social-projects/OoS-Backend/commit/{commit}"
  elif "auth" in tag:
    content = template.format("Auth server", commit)
    commit_url = f"https://github.com/ita-social-projects/OoS-Backend/commit/{commit}"
  if build['status'] == 'WORKING':
    embeds.append({
      'color': 1127128,
      'title': "🔨 DEPLOYING",
      'description': f"Deployment started at {datetime.fromisoformat(build['startTime']).strftime('%A %m %-Y, %H:%M:%S')}. Check out commit info by clicking the link.",
      'url': commit_url
    })
  elif build['status'] == 'SUCCESS':
    embeds.append({
      'color': 1027128,
      'title': "✅ SUCCESS",
      'description': f"Deployment took a {(datetime.fromisoformat(build['finishTime']) - datetime.fromisoformat(build['startTime'])).seconds} seconds. Check out new version by clicking the link.",
      'url': url
    })
  else:
    embeds.append({
      'color': 14177041,
      'title': f"❌ ERROR - {build['status']}",
      'description': "Log",
      'url': build['logUrl']
    })
  return {
    'content': content,
    'embeds': embeds
  }


def sendDiscordMessage(webhook: str, msg: DiscordMessage) -> str:
  r = requests.post(webhook, json=msg)
  if r.status_code == requests.codes.ok:
    return "Success!"
  else:
    return r.text

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
