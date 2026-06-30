> test info



test suite: `nbomber_default_test_suite_name`

test name: `nbomber_default_test_name`

session id: `2026-06-29_21.14.01_session_a0a4f28e`

> scenario stats



scenario: `get_public_gallery`

  - ok count: `20347`

  - fail count: `69`

  - all data: `0` MB

  - duration: `00:00:30`

load simulations:

  - `keep_constant`, copies: `50`, during: `00:00:30`

|step|ok stats|
|---|---|
|name|`global information`|
|request count|all = `20416`, ok = `20347`, RPS = `678.2`|
|latency (ms)|min = `3.15`, mean = `71.28`, max = `1017.02`, StdDev = `124.5`|
|latency percentile (ms)|p50 = `27.7`, p75 = `38.27`, p95 = `366.08`, p99 = `636.42`|


|step|failures stats|
|---|---|
|name|`global information`|
|request count|all = `20416`, fail = `69`, RPS = `2.3`|
|latency (ms)|min = `122.29`, mean = `358.36`, max = `798.65`, StdDev = `152.15`|
|latency percentile (ms)|p50 = `327.17`, p75 = `439.81`, p95 = `639.49`, p99 = `643.58`|


> status codes for scenario: `get_public_gallery`



|status code|count|message|
|---|---|---|
|200|20347||
|500|69|Fallo al obtener galería|


> scenario stats



scenario: `get_image_preview`

  - ok count: `12588`

  - fail count: `93`

  - all data: `1.9` MB

  - duration: `00:00:20`

load simulations:

  - `keep_constant`, copies: `100`, during: `00:00:20`

|step|ok stats|
|---|---|
|name|`global information`|
|request count|all = `12681`, ok = `12588`, RPS = `629.4`|
|latency (ms)|min = `2.47`, mean = `153.77`, max = `802.42`, StdDev = `128.96`|
|latency percentile (ms)|p50 = `98.11`, p75 = `241.92`, p95 = `406.53`, p99 = `512.51`|
|data transfer (KB)|min = `0.158`, mean = `0.158`, max = `0.158`, all = `1.9` MB|


|step|failures stats|
|---|---|
|name|`global information`|
|request count|all = `12681`, fail = `93`, RPS = `4.6`|
|latency (ms)|min = `86.57`, mean = `271.95`, max = `552.61`, StdDev = `99.82`|
|latency percentile (ms)|p50 = `267.77`, p75 = `331.26`, p95 = `422.4`, p99 = `508.16`|


> status codes for scenario: `get_image_preview`



|status code|count|message|
|---|---|---|
|404|12588||
|500|93||


