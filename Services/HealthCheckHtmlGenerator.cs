using System;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AspNetWeek3.Mvc.Services;

public static class HealthCheckHtmlGenerator
{
    public static string Generate(HealthReport report, string title)
    {
        string statusText = report.Status.ToString();
        bool isHealthy = report.Status == HealthStatus.Healthy;
        string statusColor = isHealthy ? "#006241" : "#ef4444";
        string statusBg = isHealthy ? "#d4e9e2" : "#fee2e2";
        string statusBorder = isHealthy ? "#1e3932" : "#fca5a5";

        var checksHtml = "";
        foreach (var entry in report.Entries)
        {
            var entryHealthy = entry.Value.Status == HealthStatus.Healthy;
            var entryStatusText = entry.Value.Status.ToString();
            var entryColor = entryHealthy ? "#006241" : "#ef4444";
            var entryBg = entryHealthy ? "#e8f5e9" : "#ffebee";
            
            checksHtml += $@"
            <div class='check-item'>
                <div class='check-info'>
                    <div class='check-name'>{entry.Key}</div>
                    <div class='check-desc'>{entry.Value.Description ?? "No description provided"}</div>
                </div>
                <div class='check-status' style='background: {entryBg}; color: {entryColor};'>
                    {entryStatusText}
                </div>
            </div>";
        }

        return $@"
        <!DOCTYPE html>
        <html lang='en'>
        <head>
            <meta charset='utf-8' />
            <meta name='viewport' content='width=device-width, initial-scale=1.0' />
            <title>{title} - {statusText}</title>
            <link href='https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;500;600;700;800&display=swap' rel='stylesheet'>
            <style>
                body {{
                    font-family: 'Plus Jakarta Sans', -apple-system, BlinkMacSystemFont, sans-serif;
                    background-color: #f2f0eb;
                    color: #2d2926;
                    margin: 0;
                    padding: 40px 20px;
                    display: flex;
                    justify-content: center;
                    align-items: center;
                    min-height: 100vh;
                    box-sizing: border-box;
                }}

                .wrapper {{
                    max-width: 600px;
                    width: 100%;
                    background: #ffffff;
                    border: 1px solid rgba(0, 0, 0, 0.05);
                    border-radius: 24px;
                    box-shadow: 0 10px 30px rgba(45, 41, 38, 0.04), 0 1px 3px rgba(45, 41, 38, 0.02);
                    padding: 32px;
                    box-sizing: border-box;
                    animation: fadeUp 0.6s cubic-bezier(0.16, 1, 0.3, 1) forwards;
                }}

                @keyframes fadeUp {{
                    0% {{
                        opacity: 0;
                        transform: translateY(12px);
                    }}
                    100% {{
                        opacity: 1;
                        transform: translateY(0);
                    }}
                }}

                .header {{
                    display: flex;
                    justify-content: space-between;
                    align-items: center;
                    border-bottom: 1px solid #f2f0eb;
                    padding-bottom: 24px;
                    margin-bottom: 24px;
                }}

                .brand-title {{
                    font-size: 18px;
                    font-weight: 800;
                    color: #1e3932;
                    letter-spacing: -0.02em;
                    display: flex;
                    align-items: center;
                    gap: 10px;
                }}

                .brand-logo {{
                    width: 24px;
                    height: 24px;
                    color: #006241;
                }}

                .status-badge {{
                    font-size: 10px;
                    font-weight: 800;
                    text-transform: uppercase;
                    letter-spacing: 0.1em;
                    padding: 6px 16px;
                    border-radius: 100px;
                    border: 1px solid {statusBorder};
                    background: {statusBg};
                    color: {statusColor};
                }}

                .title {{
                    font-size: 26px;
                    font-weight: 800;
                    color: #1e3932;
                    margin: 0 0 10px 0;
                    letter-spacing: -0.03em;
                }}

                .subtitle {{
                    font-size: 14.5px;
                    line-height: 1.6;
                    color: #766e65;
                    margin: 0 0 32px 0;
                }}

                .check-list {{
                    display: flex;
                    flex-direction: column;
                    gap: 16px;
                }}

                .check-item {{
                    border: 1px solid #e8e6e1;
                    border-radius: 16px;
                    padding: 16px 20px;
                    display: flex;
                    justify-content: space-between;
                    align-items: center;
                    transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
                }}

                .check-item:hover {{
                    border-color: #006241;
                    box-shadow: 0 4px 12px rgba(0, 98, 65, 0.05);
                }}

                .check-info {{
                    display: flex;
                    flex-direction: column;
                    gap: 4px;
                }}

                .check-name {{
                    font-size: 15px;
                    font-weight: 700;
                    color: #1e3932;
                }}

                .check-desc {{
                    font-size: 13px;
                    color: #766e65;
                }}

                .check-status {{
                    font-size: 10px;
                    font-weight: 800;
                    text-transform: uppercase;
                    letter-spacing: 0.05em;
                    padding: 4px 10px;
                    border-radius: 6px;
                }}

                .footer {{
                    margin-top: 32px;
                    border-top: 1px solid #f2f0eb;
                    padding-top: 20px;
                    display: flex;
                    justify-content: space-between;
                    align-items: center;
                    font-size: 12px;
                    color: #766e65;
                }}

                .btn-refresh {{
                    background: #1e3932;
                    color: #ffffff;
                    border: none;
                    padding: 8px 16px;
                    border-radius: 100px;
                    font-weight: 600;
                    font-size: 12px;
                    cursor: pointer;
                    display: inline-flex;
                    align-items: center;
                    gap: 6px;
                    transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
                    text-decoration: none;
                }}

                .btn-refresh:hover {{
                    background: #006241;
                    transform: translateY(-1px);
                }}

                .btn-refresh:active {{
                    transform: scale(0.97);
                }}
            </style>
        </head>
        <body>
            <div class='wrapper'>
                <div class='header'>
                    <div class='brand-title'>
                        <svg class='brand-logo' fill='none' viewBox='0 0 24 24' stroke='currentColor' stroke-width='2.2'>
                            <path stroke-linecap='round' stroke-linejoin='round' d='M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z' />
                        </svg>
                        Training Center System Health
                    </div>
                    <div class='status-badge'>{statusText}</div>
                </div>

                <div class='title'>{title}</div>
                <div class='subtitle'>Báo cáo tình trạng hoạt động thời gian thực của hệ thống.</div>

                <div class='check-list'>
                    {checksHtml}
                </div>

                <div class='footer'>
                    <div>Cập nhật lúc: {DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy")}</div>
                    <a href='' class='btn-refresh'>
                        <svg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 24 24' stroke-width='2.2' stroke='currentColor' style='width: 12px; height: 12px;'>
                            <path stroke-linecap='round' stroke-linejoin='round' d='M16.023 9.348h4.992v-.001M2.985 19.644v-4.992m0 0h4.992m-4.993 0 3.181 3.183a8.25 8.25 0 0 0 13.803-3.7M4.031 9.865a8.25 8.25 0 0 1 13.803-3.7l3.181 3.182m0-4.991v4.99' />
                        </svg>
                        Làm mới
                    </a>
                </div>
            </div>
        </body>
        </html>
        ";
    }
}
