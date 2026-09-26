using System;
using System.Windows;
using System.Windows.Media;
using ModernWpf;

namespace MetadataEditor.Services;

public static class ThemeService
{
    public static void ApplyTheme(string themeName)
    {
        var app = Application.Current;
        if (app == null) return;

        string bgDark, bgCard, bgInput, bgHover, sidebarBg, tagBoxBg;
        string borderDark, borderLight;
        string textPrimary, textSecondary, textMuted;
        string accentPrimary, accentHover, accentOrange, accentGreen, accentRed, accentPurple;
        string dataGridRowBg, dataGridAltRowBg;
        string bannerBg, bannerBorder, bannerFg, bannerTitleFg;
        string tagGenreBg, tagGenreFg;
        string tagStudioBg, tagStudioFg;
        string tagDirectorBg, tagDirectorFg;
        string badgeBg, badgeFg;
        ApplicationTheme modernTheme;

        switch (themeName?.ToLowerInvariant())
        {
            case "light":
                bgDark = "#F3F4F6";
                bgCard = "#FFFFFF";
                bgInput = "#FFFFFF";
                bgHover = "#E5E7EB";
                sidebarBg = "#F8FAFC";
                tagBoxBg = "#F8FAFC";
                borderDark = "#E2E8F0";
                borderLight = "#CBD5E1";
                textPrimary = "#0F172A";
                textSecondary = "#475569";
                textMuted = "#64748B";
                accentPrimary = "#0284C7";
                accentHover = "#0369A1";
                accentOrange = "#EA580C";
                accentGreen = "#16A34A";
                accentRed = "#DC2626";
                accentPurple = "#7E22CE";
                dataGridRowBg = "#FFFFFF";
                dataGridAltRowBg = "#F8FAFC";
                bannerBg = "#EFF6FF";
                bannerBorder = "#93C5FD";
                bannerFg = "#1E40AF";
                bannerTitleFg = "#1D4ED8";
                tagGenreBg = "#E0F2FE";
                tagGenreFg = "#0369A1";
                tagStudioBg = "#F3E8FF";
                tagStudioFg = "#7E22CE";
                tagDirectorBg = "#CCFBF1";
                tagDirectorFg = "#0F766E";
                badgeBg = "#F1F5F9";
                badgeFg = "#475569";
                modernTheme = ApplicationTheme.Light;
                break;

            case "midnight":
                bgDark = "#060913";
                bgCard = "#0F1626";
                bgInput = "#03050B";
                bgHover = "#1A243A";
                sidebarBg = "#060913";
                tagBoxBg = "#03050B";
                borderDark = "#1E2B45";
                borderLight = "#32456C";
                textPrimary = "#F8FAFC";
                textSecondary = "#94A3B8";
                textMuted = "#64748B";
                accentPrimary = "#60A5FA";
                accentHover = "#3B82F6";
                accentOrange = "#FB923C";
                accentGreen = "#4ADE80";
                accentRed = "#F87171";
                accentPurple = "#C084FC";
                dataGridRowBg = "#03050B";
                dataGridAltRowBg = "#080C16";
                bannerBg = "#111827";
                bannerBorder = "#3B82F6";
                bannerFg = "#FFFFFF";
                bannerTitleFg = "#93C5FD";
                tagGenreBg = "#1E293B";
                tagGenreFg = "#93C5FD";
                tagStudioBg = "#2E1E45";
                tagStudioFg = "#D8B4FE";
                tagDirectorBg = "#163030";
                tagDirectorFg = "#5EEAD4";
                badgeBg = "#0F1626";
                badgeFg = "#94A3B8";
                modernTheme = ApplicationTheme.Dark;
                break;

            case "dark":
            default:
                bgDark = "#0F172A";
                bgCard = "#1E293B";
                bgInput = "#0B1222";
                bgHover = "#2D3C52";
                sidebarBg = "#0B1222";
                tagBoxBg = "#0B1222";
                borderDark = "#334155";
                borderLight = "#475569";
                textPrimary = "#F8FAFC";
                textSecondary = "#94A3B8";
                textMuted = "#64748B";
                accentPrimary = "#38BDF8";
                accentHover = "#0EA5E9";
                accentOrange = "#F97316";
                accentGreen = "#22C55E";
                accentRed = "#EF4444";
                accentPurple = "#A855F7";
                dataGridRowBg = "#0B1222";
                dataGridAltRowBg = "#0F172A";
                bannerBg = "#1E3A8A";
                bannerBorder = "#3B82F6";
                bannerFg = "#FFFFFF";
                bannerTitleFg = "#93C5FD";
                tagGenreBg = "#1E3A8A";
                tagGenreFg = "#93C5FD";
                tagStudioBg = "#3B2E58";
                tagStudioFg = "#D8B4FE";
                tagDirectorBg = "#1C3D3D";
                tagDirectorFg = "#5EEAD4";
                badgeBg = "#1E293B";
                badgeFg = "#94A3B8";
                modernTheme = ApplicationTheme.Dark;
                break;
        }

        SetBrush(app, "BgDarkBrush", bgDark);
        SetBrush(app, "BgCardBrush", bgCard);
        SetBrush(app, "BgInputBrush", bgInput);
        SetBrush(app, "BgHoverBrush", bgHover);
        SetBrush(app, "SidebarBgBrush", sidebarBg);
        SetBrush(app, "TagBoxBgBrush", tagBoxBg);
        SetBrush(app, "BorderDarkBrush", borderDark);
        SetBrush(app, "BorderLightBrush", borderLight);
        SetBrush(app, "TextPrimaryBrush", textPrimary);
        SetBrush(app, "TextSecondaryBrush", textSecondary);
        SetBrush(app, "TextMutedBrush", textMuted);
        SetBrush(app, "AccentPrimaryBrush", accentPrimary);
        SetBrush(app, "AccentHoverBrush", accentHover);
        SetBrush(app, "AccentOrangeBrush", accentOrange);
        SetBrush(app, "AccentGreenBrush", accentGreen);
        SetBrush(app, "AccentRedBrush", accentRed);
        SetBrush(app, "AccentPurpleBrush", accentPurple);
        SetBrush(app, "DataGridRowBgBrush", dataGridRowBg);
        SetBrush(app, "DataGridAltRowBgBrush", dataGridAltRowBg);
        SetBrush(app, "BannerBgBrush", bannerBg);
        SetBrush(app, "BannerBorderBrush", bannerBorder);
        SetBrush(app, "BannerFgBrush", bannerFg);
        SetBrush(app, "BannerTitleFgBrush", bannerTitleFg);
        SetBrush(app, "TagGenreBgBrush", tagGenreBg);
        SetBrush(app, "TagGenreFgBrush", tagGenreFg);
        SetBrush(app, "TagStudioBgBrush", tagStudioBg);
        SetBrush(app, "TagStudioFgBrush", tagStudioFg);
        SetBrush(app, "TagDirectorBgBrush", tagDirectorBg);
        SetBrush(app, "TagDirectorFgBrush", tagDirectorFg);
        SetBrush(app, "BadgeBgBrush", badgeBg);
        SetBrush(app, "BadgeFgBrush", badgeFg);

        try
        {
            ThemeManager.Current.ApplicationTheme = modernTheme;

            var elementTheme = modernTheme == ApplicationTheme.Light ? ElementTheme.Light : ElementTheme.Dark;
            foreach (Window win in app.Windows)
            {
                ThemeManager.SetRequestedTheme(win, elementTheme);
            }
        }
        catch
        {
            // Ignore if theme manager is not ready yet
        }
    }

    private static void SetBrush(Application app, string resourceKey, string hexColor)
    {
        try
        {
            var color = (Color)ColorConverter.ConvertFromString(hexColor);
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            app.Resources[resourceKey] = brush;
        }
        catch
        {
            // Silently continue
        }
    }
}

