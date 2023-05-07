import base64

import main

import os


def test_cloudbuild_subscribe(capsys):
    event = type('cloudevent', (object,), {"attributes": {}, "data": {}})

    with open('test.json', 'r') as file:
        data = file.read().rstrip()

    event.data = {
        "message": {
            "data": base64.b64encode(bytes(data, 'UTF-8')),
        }
    }

    os.environ["WEBHOOK_URL"] = "https://httpbin.org/post"

    main.subscribe(event)

    out, _ = capsys.readouterr()
    assert "Success!" in out
