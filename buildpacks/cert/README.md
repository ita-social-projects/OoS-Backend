# External CAs Buildpack

A buildpack that downloads, extracts and configures environment variables of external CAs for Encryption service runtime.

Sets `EUSign__CA__JsonPath` and `EUSign__CA__CertificatesPath` variables, respectively.

Requires:
- `BP_CA_JSON_URL` variable set with a link to latest CA JSON file.
- `BP_CA_P7B_URL` variable set with a link to latest CA `*.p7b` file.