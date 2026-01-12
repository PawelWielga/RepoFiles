# manifest.json schema

Manifest is a JSON array of entries with the following fields:

- `filename` (string, required): File name or relative path.
- `url` (string, optional): Absolute URL to download. If missing, provider builds a URL.
- `size` (number, required): File size in bytes.
- `modifydate` (string, optional): ISO-8601 date/time. Used for update decisions.
- `metadata` (string, optional): JSON string with additional data for consumers to deserialize.

Example:

```json
[
  {
    "filename": "data.db",
    "url": "https://raw.githubusercontent.com/owner/repo/main/data.db",
    "size": 123456,
    "modifydate": "2024-01-01T12:00:00Z",
    "metadata": "{\"minAppVersion\":\"1.2.3\",\"note\":\"Optional note\"}"
  }
]
```
