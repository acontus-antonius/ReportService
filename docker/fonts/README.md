Place licensed custom font files for the Docker image in this directory.

For Arial on Windows, copy the relevant files from `C:\Windows\Fonts`, for example:

- `arial.ttf`
- `arialbd.ttf`
- `ariali.ttf`
- `arialbi.ttf`

The Dockerfile copies this directory to `/usr/local/share/fonts/truetype/custom/`
and refreshes `fontconfig` during the image build.
