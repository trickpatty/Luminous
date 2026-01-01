#!/bin/bash
# Luminous Display - Kiosk Mode Setup Script (Linux)
#
# This script configures a minimal X session for kiosk mode on Raspberry Pi
# or other Linux single-board computers. Run with sudo.

set -e

echo "Luminous Display - Kiosk Mode Setup"
echo "===================================="
echo ""

# Check if running as root
if [ "$EUID" -ne 0 ]; then
    echo "Please run as root (sudo)"
    exit 1
fi

# Detect platform
PLATFORM="unknown"
if [ -f /proc/device-tree/model ]; then
    MODEL=$(cat /proc/device-tree/model)
    if [[ "$MODEL" == *"Raspberry Pi"* ]]; then
        PLATFORM="raspberry-pi"
        echo "Detected: Raspberry Pi"
    fi
else
    PLATFORM="generic-linux"
    echo "Detected: Generic Linux"
fi

# Create kiosk user if it doesn't exist
KIOSK_USER="luminous"
if ! id "$KIOSK_USER" &>/dev/null; then
    echo "Creating kiosk user: $KIOSK_USER"
    useradd -m -G video,audio,input,tty "$KIOSK_USER"
fi

# Install dependencies
echo "Installing dependencies..."
apt-get update
apt-get install -y --no-install-recommends \
    xorg \
    xserver-xorg-video-fbdev \
    x11-xserver-utils \
    unclutter \
    chromium-browser 2>/dev/null || apt-get install -y --no-install-recommends chromium

# Create X session configuration
echo "Configuring X session..."

XINITRC="/home/$KIOSK_USER/.xinitrc"
cat > "$XINITRC" << 'EOF'
#!/bin/bash
# Luminous Display - Kiosk X Session

# Disable screen saver and power management
xset s off
xset -dpms
xset s noblank

# Hide cursor after 1 second of inactivity
unclutter -idle 1 -root &

# Wait for X to be ready
sleep 2

# Set display rotation (0=normal, 1=right, 2=inverted, 3=left)
# Uncomment for portrait mode on landscape displays:
# xrandr --output HDMI-1 --rotate right

# Start Luminous Display
export LUMINOUS_KIOSK=true
exec /opt/luminous-display/luminous-display
EOF

chmod +x "$XINITRC"
chown "$KIOSK_USER:$KIOSK_USER" "$XINITRC"

# Create autologin configuration
echo "Configuring auto-login..."

if [ "$PLATFORM" = "raspberry-pi" ]; then
    # Raspberry Pi OS uses a different autologin method
    raspi-config nonint do_boot_behaviour B2  # Console autologin
    cat >> /etc/rc.local << EOF

# Start Luminous Display kiosk
su - $KIOSK_USER -c 'startx' &
EOF
else
    # Generic Linux with getty autologin
    mkdir -p /etc/systemd/system/getty@tty1.service.d
    cat > /etc/systemd/system/getty@tty1.service.d/autologin.conf << EOF
[Service]
ExecStart=
ExecStart=-/sbin/agetty --autologin $KIOSK_USER --noclear %I \$TERM
EOF

    # Add startx to bash profile
    cat >> "/home/$KIOSK_USER/.bash_profile" << 'EOF'
if [ -z "$DISPLAY" ] && [ "$(tty)" = "/dev/tty1" ]; then
    exec startx
fi
EOF
    chown "$KIOSK_USER:$KIOSK_USER" "/home/$KIOSK_USER/.bash_profile"
fi

# Disable screen blanking in console
cat >> /etc/rc.local << 'EOF'
setterm -blank 0 -powerdown 0 -powersave off
EOF

echo ""
echo "Kiosk mode setup complete!"
echo ""
echo "Installation directory: /opt/luminous-display"
echo "Kiosk user: $KIOSK_USER"
echo ""
echo "Next steps:"
echo "1. Copy the Luminous Display app to /opt/luminous-display"
echo "2. Reboot the system"
echo ""
echo "The display will automatically start in kiosk mode after reboot."
