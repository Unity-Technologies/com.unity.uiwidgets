#!/usr/bin/env bash
#set -xeuo pipefail
#set -x

printf "Preparing samples:\n"

cd com.unity.uiwidgets

mkdir Samples
cd Samples

# skip copying samples
# cp -r ../../Samples/UIWidgetsSamples_2019_4/Assets UIWidgetsSamples

echo "Finished the sample preparation."
