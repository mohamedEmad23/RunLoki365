#!/bin/bash
# Build and package RunLoki365 for Debian/Ubuntu

set -e

VERSION="1.0.0"
ARCH="amd64"
PACKAGE_NAME="runloki365_${VERSION}_${ARCH}"
BUILD_DIR="$(pwd)/build/${PACKAGE_NAME}"

echo "🐧 Building RunLoki365 v${VERSION}..."
echo ""

# Clean previous build
rm -rf build bin/Release
mkdir -p "${BUILD_DIR}"

# Publish the application
echo "📦 Publishing application..."
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:Version=${VERSION}

# Create directory structure
mkdir -p "${BUILD_DIR}/opt/runloki365"
mkdir -p "${BUILD_DIR}/usr/share/applications"
mkdir -p "${BUILD_DIR}/DEBIAN"

# Copy executable
cp bin/Release/net9.0/linux-x64/publish/runloki365 "${BUILD_DIR}/opt/runloki365/"
chmod +x "${BUILD_DIR}/opt/runloki365/runloki365"

# Copy desktop file
cp debian/runloki365.desktop "${BUILD_DIR}/usr/share/applications/"

# Copy Debian control files
cp debian/control "${BUILD_DIR}/DEBIAN/"
cp debian/postinst "${BUILD_DIR}/DEBIAN/"
cp debian/prerm "${BUILD_DIR}/DEBIAN/"
chmod 755 "${BUILD_DIR}/DEBIAN/postinst"
chmod 755 "${BUILD_DIR}/DEBIAN/prerm"

# Create copyright file
cat > "${BUILD_DIR}/DEBIAN/copyright" << 'EOF'
Format: https://www.debian.org/doc/packaging-manuals/copyright-format/1.0/
Upstream-Name: runloki365
Source: https://github.com/mohamedEmad23/RunLoki365

Files: *
Copyright: 2025 Mohammed Emad (mohamedEmad23)
License: MIT
 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:
 .
 The above copyright notice and this permission notice shall be included in all
 copies or substantial portions of the Software.
 .
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 SOFTWARE.
EOF

# Calculate installed size
INSTALLED_SIZE=$(du -sk "${BUILD_DIR}/opt" | cut -f1)
echo "Installed-Size: ${INSTALLED_SIZE}" >> "${BUILD_DIR}/DEBIAN/control"

# Build the package
echo "🔨 Building .deb package..."
dpkg-deb --build --root-owner-group "${BUILD_DIR}"

# Move to build directory
mv "${BUILD_DIR}.deb" "build/${PACKAGE_NAME}.deb"

echo ""
echo "✅ Build complete!"
echo "   Package: build/${PACKAGE_NAME}.deb"
echo "   Size: $(du -h "build/${PACKAGE_NAME}.deb" | cut -f1)"
echo ""
echo "📥 To install:"
echo "   sudo dpkg -i build/${PACKAGE_NAME}.deb"
echo "   sudo apt-get install -f"
