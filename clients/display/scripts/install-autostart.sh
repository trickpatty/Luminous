#!/bin/bash
# Luminous Display - Auto-start Installation Script (Linux)
#
# This script configures the display app to start automatically on boot.
# Supports systemd (for servers/kiosks) and XDG autostart (for desktops).

set -e

APP_NAME="luminous-display"
APP_PATH="${LUMINOUS_INSTALL_PATH:-/opt/luminous-display}"
CURRENT_USER=$(whoami)

echo "Luminous Display - Auto-start Configuration"
echo "============================================"

# Check if running as root for systemd service
if [ "$EUID" -eq 0 ]; then
    echo "Installing systemd service..."

    # Create systemd service file
    cat > /etc/systemd/system/luminous-display.service << EOF
[Unit]
Description=Luminous Display - Family Command Center
After=graphical.target network.target
Wants=graphical.target

[Service]
Type=simple
User=${SUDO_USER:-$CURRENT_USER}
Environment=DISPLAY=:0
Environment=XAUTHORITY=/home/${SUDO_USER:-$CURRENT_USER}/.Xauthority
Environment=LUMINOUS_KIOSK=true
ExecStart=${APP_PATH}/luminous-display
Restart=always
RestartSec=3
# Give it time to start X if needed
ExecStartPre=/bin/sleep 5

[Install]
WantedBy=graphical.target
EOF

    # Reload systemd and enable service
    systemctl daemon-reload
    systemctl enable luminous-display.service
    echo "Systemd service installed and enabled."
    echo "Start with: sudo systemctl start luminous-display"

else
    echo "Installing XDG autostart entry for user: $CURRENT_USER"

    # Create XDG autostart directory if it doesn't exist
    AUTOSTART_DIR="$HOME/.config/autostart"
    mkdir -p "$AUTOSTART_DIR"

    # Create desktop entry
    cat > "$AUTOSTART_DIR/luminous-display.desktop" << EOF
[Desktop Entry]
Type=Application
Name=Luminous Display
Comment=Family Command Center
Exec=${APP_PATH}/luminous-display
Icon=${APP_PATH}/resources/app/assets/icons/icon.png
Terminal=false
Categories=Utility;
StartupNotify=false
X-GNOME-Autostart-enabled=true
X-GNOME-Autostart-Delay=5
EOF

    chmod +x "$AUTOSTART_DIR/luminous-display.desktop"
    echo "XDG autostart entry created at: $AUTOSTART_DIR/luminous-display.desktop"
fi

echo ""
echo "Auto-start configuration complete!"
echo "The display app will start automatically on next boot."
