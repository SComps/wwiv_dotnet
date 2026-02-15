#!/bin/bash
# Build script for Linux
# Builds self-contained AOT-compiled binaries for Linux

CONFIGURATION="${1:-Release}"
ARCHITECTURE="${2:-x64}"

echo "═══════════════════════════════════════════════════════════"
echo "  WWIV .NET - Linux Build Script (AOT + Self-Contained)"
echo "═══════════════════════════════════════════════════════════"
echo ""

# Clean previous builds
echo "[1/4] Cleaning previous builds..."
dotnet clean -c "$CONFIGURATION"
if [ $? -ne 0 ]; then
    echo "Clean failed!"
    exit 1
fi

# Restore dependencies
echo "[2/4] Restoring dependencies..."
dotnet restore
if [ $? -ne 0 ]; then
    echo "Restore failed!"
    exit 1
fi

# Build
echo "[3/4] Building..."
dotnet build -c "$CONFIGURATION"
if [ $? -ne 0 ]; then
    echo "Build failed!"
    exit 1
fi

# Publish AOT self-contained for Linux
echo "[4/4] Publishing AOT self-contained binary for Linux..."
RUNTIME_ID="linux-$ARCHITECTURE"
OUTPUT_PATH="publish/linux-$ARCHITECTURE"

dotnet publish wwiv3270/wwiv3270.vbproj \
    -c "$CONFIGURATION" \
    -r "$RUNTIME_ID" \
    --self-contained \
    -p:PublishAot=true \
    -p:PublishTrimmed=true \
    -p:PublishSingleFile=false \
    -o "$OUTPUT_PATH"

if [ $? -ne 0 ]; then
    echo "Publish failed!"
    exit 1
fi

echo ""
echo "═══════════════════════════════════════════════════════════"
echo "  Build Complete!"
echo "═══════════════════════════════════════════════════════════"
echo ""
echo "Output directory: $OUTPUT_PATH"
echo "Executable: $OUTPUT_PATH/wwiv3270"
echo ""

# Show file size
if [ -f "$OUTPUT_PATH/wwiv3270" ]; then
    FILE_SIZE=$(du -h "$OUTPUT_PATH/wwiv3270" | cut -f1)
    echo "Binary size: $FILE_SIZE"
    
    # Make executable
    chmod +x "$OUTPUT_PATH/wwiv3270"
    echo "Executable permissions set"
fi

echo ""
echo "To run: cd $OUTPUT_PATH && ./wwiv3270"
