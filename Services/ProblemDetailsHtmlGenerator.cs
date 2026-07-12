using System;

namespace AspNetWeek3.Mvc.Services;

public static class ProblemDetailsHtmlGenerator
{
    public static string Generate(int statusCode, string title, string detail, string type, string instance, string traceId, string timestamp, string? errorCode = null)
    {
        string errorCodeCard = !string.IsNullOrEmpty(errorCode) ? $"""
                                    <div class="data-card-outer">
                                        <div class="data-card-inner">
                                            <div class="data-card-header">
                                                <span class="data-label">Error Code</span>
                                            </div>
                                            <p class="data-value" style="color: #ef4444 !important; font-weight: 700 !important;">{errorCode}</p>
                                        </div>
                                    </div>
        """ : "";

        string statusText = statusCode switch
        {
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            500 => "Internal Server Error",
            _ => "Error"
        };

        bool isSevere = statusCode >= 500;
        string badgeClass = isSevere ? "eyebrow" : "eyebrow warning";
        string statusLabel = isSevere ? "CRITICAL ERROR" : "DIAGNOSTIC ENCOUNTERED";

        return $$"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="utf-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1.0" />
            <title>API Diagnostics - {{statusCode}} {{statusText}}</title>
            <link href="https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;500;600;700;800&family=JetBrains+Mono:wght@400;500;600&display=swap" rel="stylesheet">
            <style>
                :root {
                    --sb-green: #00754a;
                    --sb-dark-green: #1e3932;
                    --sb-mint: #d4e9e2;
                    --sb-cream: #f2f0eb;
                    --text-body: #2d2926;
                    --text-muted: #766e65;
                }

                html, body {
                    height: 100%;
                    margin: 0;
                    padding: 0;
                    overflow: hidden;
                }
                
                body {
                    font-family: 'Plus Jakarta Sans', -apple-system, BlinkMacSystemFont, sans-serif;
                    background-color: var(--sb-cream);
                    color: var(--text-body);
                    display: flex;
                    justify-content: center;
                    align-items: center;
                    padding: 24px;
                    box-sizing: border-box;
                }

                .outer-wrapper {
                    max-width: 1200px;
                    width: 100%;
                    height: 100%;
                    max-height: calc(100vh - 48px);
                    background: rgba(0, 0, 0, 0.015);
                    border: 1px solid rgba(0, 0, 0, 0.03);
                    border-radius: 24px;
                    padding: 8px;
                    box-shadow: 0 16px 40px rgba(0, 0, 0, 0.03);
                    animation: fadeUp 0.6s cubic-bezier(0.16, 1, 0.3, 1) forwards;
                    display: flex;
                    flex-direction: column;
                    box-sizing: border-box;
                }

                @keyframes fadeUp {
                    0% {
                        opacity: 0;
                        transform: translateY(12px);
                    }
                    100% {
                        opacity: 1;
                        transform: translateY(0);
                    }
                }

                .inner-container {
                    background: #ffffff;
                    border: 1px solid rgba(0, 0, 0, 0.05);
                    border-radius: 18px;
                    box-shadow: 0 4px 16px rgba(0, 0, 0, 0.01);
                    overflow: hidden;
                    display: flex;
                    flex-direction: column;
                    height: 100%;
                }

                .console-header {
                    display: flex;
                    justify-content: space-between;
                    align-items: center;
                    padding: 16px 28px;
                    background: rgba(0, 117, 74, 0.03);
                    border-bottom: 1px solid rgba(0, 117, 74, 0.08);
                    flex-shrink: 0;
                }

                .window-actions {
                    display: flex;
                    gap: 8px;
                }
                
                .window-dot {
                    width: 12px;
                    height: 12px;
                    border-radius: 50%;
                }
                .dot-red { background: #ef4444; }
                .dot-yellow { background: #eab308; }
                .dot-green { background: #22c55e; }

                .console-grid {
                    display: grid;
                    grid-template-columns: 1fr 1.2fr;
                    gap: 0;
                    flex-grow: 1;
                    overflow: hidden;
                }

                @media (max-width: 920px) {
                    .console-grid {
                        grid-template-columns: 1fr;
                        overflow-y: auto;
                    }
                }

                .left-panel {
                    padding: 28px 32px;
                    border-right: 1px solid rgba(0, 117, 74, 0.08);
                    display: flex;
                    flex-direction: column;
                    justify-content: space-between;
                    overflow-y: auto;
                    height: 100%;
                    box-sizing: border-box;
                    background: #ffffff;
                }

                @media (max-width: 920px) {
                    .left-panel {
                        border-right: none;
                        border-bottom: 1px solid rgba(0, 117, 74, 0.08);
                        height: auto;
                        overflow-y: visible;
                    }
                }

                .right-panel {
                    padding: 28px 32px;
                    background: #faf9f6;
                    display: flex;
                    flex-direction: column;
                    height: 100%;
                    overflow: hidden;
                    box-sizing: border-box;
                }

                @media (max-width: 920px) {
                    .right-panel {
                        height: 500px;
                        overflow: visible;
                    }
                }

                .eyebrow {
                    display: inline-flex;
                    align-items: center;
                    background: rgba(220, 53, 69, 0.06);
                    border: 1px solid rgba(220, 53, 69, 0.15);
                    color: #dc3545;
                    font-size: 9.5px;
                    font-weight: 700;
                    text-transform: uppercase;
                    letter-spacing: 0.2em;
                    padding: 4px 12px;
                    border-radius: 100px;
                    margin-bottom: 16px;
                    width: fit-content;
                }

                .eyebrow.warning {
                    background: rgba(217, 119, 6, 0.06);
                    border: 1px solid rgba(217, 119, 6, 0.15);
                    color: #d97706;
                }

                .error-title {
                    font-size: 24px;
                    font-weight: 800;
                    color: var(--sb-dark-green);
                    margin: 0 0 10px 0;
                    letter-spacing: -0.02em;
                    line-height: 1.2;
                }

                .error-detail {
                    font-size: 14px;
                    line-height: 1.5;
                    color: var(--text-muted);
                    margin: 0 0 24px 0;
                }

                .data-group {
                    display: flex;
                    flex-direction: column;
                    gap: 12px;
                }

                .data-card-outer {
                    background: rgba(0, 0, 0, 0.01);
                    border: 1px solid rgba(0, 0, 0, 0.02);
                    border-radius: 12px;
                    padding: 3px;
                }

                .data-card-inner {
                    background: #ffffff;
                    border: 1px solid rgba(0, 0, 0, 0.04);
                    border-radius: 9px;
                    padding: 10px 14px;
                    display: flex;
                    flex-direction: column;
                    gap: 4px;
                    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.01);
                }

                .data-card-header {
                    display: flex;
                    justify-content: space-between;
                    align-items: center;
                    width: 100%;
                }

                .data-label {
                    font-size: 9px;
                    font-weight: 700;
                    text-transform: uppercase;
                    color: var(--text-muted);
                    letter-spacing: 0.15em;
                }

                .data-value {
                    font-size: 12px;
                    font-family: 'JetBrains Mono', monospace;
                    color: var(--sb-dark-green);
                    word-break: break-all;
                    margin: 0;
                    flex-grow: 1;
                }

                .copy-button {
                    background: rgba(0, 117, 74, 0.06);
                    border: 1px solid rgba(0, 117, 74, 0.12);
                    color: var(--sb-green);
                    cursor: pointer;
                    padding: 4px;
                    border-radius: 6px;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
                }

                .copy-button:hover {
                    color: #ffffff;
                    background: var(--sb-green);
                    border-color: var(--sb-dark-green);
                    transform: translateY(-1px);
                }

                .copy-button:active {
                    transform: scale(0.96);
                }

                .action-buttons {
                    margin-top: 24px;
                    display: flex;
                    gap: 12px;
                }

                .btn-primary-pill {
                    background: var(--sb-green);
                    color: white;
                    padding: 6px 6px 6px 20px;
                    border-radius: 100px;
                    font-weight: 600;
                    font-size: 13.5px;
                    text-decoration: none;
                    display: inline-flex;
                    align-items: center;
                    justify-content: space-between;
                    transition: all 0.5s cubic-bezier(0.16, 1, 0.3, 1);
                    border: 1px solid var(--sb-dark-green);
                    box-shadow: 0 4px 12px rgba(0, 117, 74, 0.15);
                    cursor: pointer;
                    gap: 12px;
                }

                .btn-primary-pill:hover {
                    background: var(--sb-dark-green);
                    box-shadow: 0 6px 16px rgba(0, 117, 74, 0.25);
                    transform: translateY(-2px);
                }

                .btn-primary-pill:active {
                    transform: scale(0.98);
                }

                .btn-icon-circle {
                    width: 28px;
                    height: 28px;
                    border-radius: 50%;
                    background: rgba(255, 255, 255, 0.2);
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
                }

                .btn-primary-pill:hover .btn-icon-circle {
                    transform: rotate(45deg);
                    background: rgba(255, 255, 255, 0.3);
                }

                .btn-secondary-pill {
                    background: #ffffff;
                    color: var(--sb-dark-green);
                    border: 1px solid rgba(0, 117, 74, 0.2);
                    padding: 10px 24px;
                    border-radius: 100px;
                    font-weight: 600;
                    font-size: 13.5px;
                    text-decoration: none;
                    display: inline-flex;
                    align-items: center;
                    justify-content: center;
                    transition: all 0.5s cubic-bezier(0.16, 1, 0.3, 1);
                }

                .btn-secondary-pill:hover {
                    background: rgba(0, 117, 74, 0.04);
                    border-color: var(--sb-green);
                    transform: translateY(-2px);
                }

                .btn-secondary-pill:active {
                    transform: scale(0.98);
                }

                .json-pane-header {
                    display: flex;
                    justify-content: space-between;
                    align-items: center;
                    margin-bottom: 16px;
                    flex-shrink: 0;
                }

                .json-pane-title {
                    font-size: 9.5px;
                    font-weight: 700;
                    color: var(--text-muted);
                    text-transform: uppercase;
                    letter-spacing: 0.15em;
                }

                .editor-wrapper-outer {
                    background: rgba(0, 0, 0, 0.015);
                    border: 1px solid rgba(0, 0, 0, 0.03);
                    border-radius: 16px;
                    padding: 3px;
                    flex-grow: 1;
                    display: flex;
                    flex-direction: column;
                    overflow: hidden;
                }

                .editor-wrapper-inner {
                    background: #ffffff;
                    border: 1px solid rgba(0, 117, 74, 0.08);
                    border-radius: 13px;
                    padding: 20px;
                    flex-grow: 1;
                    display: flex;
                    flex-direction: column;
                    box-shadow: inset 0 1px 4px rgba(0,0,0,0.01);
                    overflow-y: auto;
                }

                .json-display {
                    flex-grow: 1;
                    margin: 0;
                    font-family: 'JetBrains Mono', monospace;
                    font-size: 13px;
                    line-height: 1.6;
                    color: var(--sb-dark-green);
                    overflow-x: auto;
                    white-space: pre-wrap;
                    outline: none;
                }

                /* Syntax Highlighting on light theme */
                .json-key { color: #c2410c; font-weight: 600; }
                .json-string { color: #16a34a; }
                .json-number { color: #2563eb; }
                .json-boolean { color: #d97706; }
                .json-null { color: #64748b; }

                .toast {
                    position: fixed;
                    bottom: 30px;
                    left: 50%;
                    transform: translateX(-50%) translateY(100px);
                    background: var(--sb-dark-green);
                    color: white;
                    padding: 12px 24px;
                    border-radius: 100px;
                    font-size: 14px;
                    font-weight: 600;
                    box-shadow: 0 10px 25px rgba(0, 0, 0, 0.15);
                    opacity: 0;
                    transition: all 0.6s cubic-bezier(0.16, 1, 0.3, 1);
                    z-index: 100;
                    border: 1px solid rgba(255, 255, 255, 0.1);
                    backdrop-filter: blur(8px);
                    pointer-events: none;
                }
                .toast.show {
                    transform: translateX(-50%) translateY(0);
                    opacity: 1;
                }
            </style>
        </head>
        <body>
            <div class="outer-wrapper">
                <div class="inner-container">
                    <div class="console-header">
                        <div class="window-actions">
                            <span class="window-dot dot-red"></span>
                            <span class="window-dot dot-yellow"></span>
                            <span class="window-dot dot-green"></span>
                        </div>
                        <div style="font-size: 10px; color: var(--sb-green); font-family: 'JetBrains Mono', monospace; font-weight: 700; letter-spacing: 0.05em;">
                            RFC 7807 DIAGNOSTIC CONSOLE
                        </div>
                    </div>
                    
                    <div class="console-grid">
                        <div class="left-panel">
                            <div>
                                <span class="{{badgeClass}}">{{statusCode}} {{statusText}} • {{statusLabel}}</span>
                                <div class="error-title">{{title}}</div>
                                <div class="error-detail">{{detail}}</div>
                                
                                <div class="data-group">
                                    {{errorCodeCard}}
                                    <div class="data-card-outer">
                                        <div class="data-card-inner">
                                            <div class="data-card-header">
                                                <span class="data-label">Error Type Schema</span>
                                            </div>
                                            <p class="data-value">{{type}}</p>
                                        </div>
                                    </div>
                                    <div class="data-card-outer">
                                        <div class="data-card-inner">
                                            <div class="data-card-header">
                                                <span class="data-label">Request Instance</span>
                                            </div>
                                            <p class="data-value">{{instance}}</p>
                                        </div>
                                    </div>
                                    <div class="data-card-outer">
                                        <div class="data-card-inner">
                                            <div class="data-card-header">
                                                <span class="data-label">Trace Identifier</span>
                                                <button class="copy-button" onclick="copyText('trace-id-val')" title="Copy Trace ID">
                                                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.8" stroke="currentColor" style="width: 14px; height: 14px;">
                                                        <path stroke-linecap="round" stroke-linejoin="round" d="M8.25 7.5V6.108c0-1.135.845-2.098 1.976-2.192.373-.03.748-.057 1.123-.08M15.75 18H18a2.25 2.25 0 0 0 2.25-2.25V6.108c0-1.135-.845-2.098-1.976-2.192a48.424 48.424 0 0 0-1.123-.08M15.75 18.75v-1.875a3.375 3.375 0 0 0-3.375-3.375h-1.5a1.125 1.125 0 0 1-1.125-1.125v-1.5A3.375 3.375 0 0 0 6.375 7.5H5.25m11.9-3.664A2.251 2.251 0 0 0 15 2.25h-1.5a2.251 2.251 0 0 0-2.15 1.586m5.8 0c.065.21.1.433.1.664v.75h-6V4.5c0-.231.035-.454.1-.664M6.75 7.5H4.875c-.621 0-1.125.504-1.125 1.125v12c0 .621.504 1.125 1.125 1.125h9.75c.621 0 1.125-.504 1.125-1.125V16.5a9 9 0 0 0-9-9Z" />
                                                    </svg>
                                                </button>
                                            </div>
                                            <p class="data-value" id="trace-id-val">{{traceId}}</p>
                                        </div>
                                    </div>
                                    <div class="data-card-outer">
                                        <div class="data-card-inner">
                                            <div class="data-card-header">
                                                <span class="data-label">Timestamp (UTC)</span>
                                            </div>
                                            <p class="data-value">{{timestamp}}</p>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            
                            <div class="action-buttons">
                                <a href="/" class="btn-primary-pill">
                                    Quay lại Dashboard
                                    <span class="btn-icon-circle">
                                        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor" style="width: 12px; height: 12px;">
                                            <path stroke-linecap="round" stroke-linejoin="round" d="M13.5 4.5 21 12m0 0-7.5 7.5M21 12H3" />
                                        </svg>
                                    </span>
                                </a>
                                <a href="?format=json" class="btn-secondary-pill">Xem Raw JSON</a>
                            </div>
                        </div>
                        
                        <div class="right-panel">
                            <div class="json-pane-header">
                                <span class="json-pane-title">Structured JSON Output</span>
                                <button class="copy-button" onclick="copyJson()" title="Copy JSON Output" style="padding: 6px 12px; font-size: 11px; display: inline-flex; align-items: center; gap: 6px; border-radius: 100px; font-family: inherit; font-weight: 600;">
                                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.8" stroke="currentColor" style="width: 13px; height: 13px;">
                                        <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 17.25v3.375c0 .621-.504 1.125-1.125 1.125h-9.75a1.125 1.125 0 0 1-1.125-1.125V7.875c0-.621.504-1.125 1.125-1.125H6.75a9.06 9.06 0 0 1 1.5.124m7.5 10.376A8.965 8.965 0 0 0 12 12.75c-.497 0-.982.04-1.455.12m8.678 3.664a9.053 9.053 0 0 0-1.455-1.31M16.5 7.5v3.75a1.125 1.125 0 0 1-1.125 1.125H11.25m4.5-4.875A2.25 2.25 0 0 0 13.5 2.25h-1.5a2.25 2.25 0 0 0-2.25 2.25v.75h6v-.75Z" />
                                    </svg>
                                    Copy JSON
                                </button>
                            </div>
                            <div class="editor-wrapper-outer">
                                <div class="editor-wrapper-inner">
                                    <pre class="json-display" id="json-output"></pre>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div id="toast" class="toast">Trace ID copied!</div>

            <script>
                const rawJsonObj = {
                    "type": "{{type}}",
                    "title": "{{title}}",
                    "status": {{statusCode}},
                    "detail": "{{detail}}",
                    "instance": "{{instance}}",
                    "traceId": "{{traceId}}",
                    "timestamp": "{{timestamp}}"{{(string.IsNullOrEmpty(errorCode) ? "" : $",\n                    \"errorCode\": \"{errorCode}\"")}}
                };

                function syntaxHighlight(json) {
                    if (typeof json != 'string') {
                         json = JSON.stringify(json, undefined, 4);
                    }
                    json = json.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
                    return json.replace(/("(\\u[a-zA-Z0-9]{4}|\\[^u]|[^\\"])*"(\s*:)?|\b(true|false|null)\b|-?\d+(?:\.\d*)?(?:[eE][+-]?\d+)?)/g, function (match) {
                        var cls = 'number';
                        if (/^"/.test(match)) {
                            if (/:$/.test(match)) {
                                cls = 'key';
                            } else {
                                cls = 'string';
                            }
                        } else if (/true|false/.test(match)) {
                            cls = 'boolean';
                        } else if (/null/.test(match)) {
                            cls = 'null';
                        }
                        return '<span class="json-' + cls + '">' + match + '</span>';
                    });
                }

                document.getElementById('json-output').innerHTML = syntaxHighlight(rawJsonObj);

                function showToast(message) {
                    const toast = document.getElementById('toast');
                    toast.innerText = message;
                    toast.classList.add('show');
                    setTimeout(() => {
                        toast.classList.remove('show');
                    }, 2500);
                }

                function copyText(elementId) {
                    const text = document.getElementById(elementId).innerText;
                    navigator.clipboard.writeText(text).then(() => {
                        showToast('Copied Trace ID to clipboard!');
                    });
                }

                function copyJson() {
                    navigator.clipboard.writeText(JSON.stringify(rawJsonObj, null, 4)).then(() => {
                        showToast('Copied full JSON to clipboard!');
                    });
                }
            </script>
        </body>
        </html>
        """;
    }
}
