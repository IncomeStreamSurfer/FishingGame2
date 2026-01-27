using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Email service for Poll Booth submissions
/// Saves submissions locally and is ready for Gmail SMTP integration
///
/// GMAIL SETUP INSTRUCTIONS:
/// =========================
/// 1. Create a Gmail account for receiving feedback (e.g., fishordie.feedback@gmail.com)
/// 2. Enable 2-Factor Authentication on the Gmail account
/// 3. Generate an App Password: Google Account > Security > App Passwords
/// 4. Create a config file at: StreamingAssets/EmailConfig.json
/// 5. Add your credentials (see EmailConfig structure below)
/// 6. Set USE_EMAIL_SERVICE = true in this script
///
/// The EmailConfig.json should contain:
/// {
///     "smtpServer": "smtp.gmail.com",
///     "smtpPort": 587,
///     "senderEmail": "your-game-email@gmail.com",
///     "senderPassword": "your-app-password",
///     "recipientEmail": "where-to-receive@gmail.com",
///     "enableSsl": true
/// }
/// </summary>
public static class PollBoothEmailService
{
    // Toggle this to enable email sending (requires EmailConfig.json setup)
    private static readonly bool USE_EMAIL_SERVICE = false;

    // Local storage paths
    private static string SubmissionsFolder => Path.Combine(Application.persistentDataPath, "PollSubmissions");
    private static string PendingQueueFile => Path.Combine(SubmissionsFolder, "pending_queue.json");
    private static string SubmissionsLogFile => Path.Combine(SubmissionsFolder, "submissions_log.json");

    // Email config path (in StreamingAssets for easy access)
    private static string EmailConfigPath => Path.Combine(Application.streamingAssetsPath, "EmailConfig.json");

    /// <summary>
    /// Save a submission locally and optionally send via email
    /// </summary>
    public static void SaveSubmission(PollSubmission submission)
    {
        try
        {
            // Ensure folder exists
            if (!Directory.Exists(SubmissionsFolder))
            {
                Directory.CreateDirectory(SubmissionsFolder);
            }

            // Save to individual file (for backup)
            string fileName = $"submission_{DateTime.Now:yyyyMMdd_HHmmss}_{submission.category.Replace(" ", "_")}.json";
            string filePath = Path.Combine(SubmissionsFolder, fileName);
            string json = JsonUtility.ToJson(submission, true);
            File.WriteAllText(filePath, json);

            // Add to submissions log
            AddToSubmissionsLog(submission);

            // Add to pending queue for email
            AddToPendingQueue(submission);

            Debug.Log($"[PollBooth] Submission saved: {filePath}");

            // Attempt to send email if enabled
            if (USE_EMAIL_SERVICE)
            {
                TrySendEmail(submission);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[PollBooth] Failed to save submission: {e.Message}");
        }
    }

    /// <summary>
    /// Add submission to the log file (appends to array)
    /// </summary>
    private static void AddToSubmissionsLog(PollSubmission submission)
    {
        try
        {
            SubmissionsLog log;

            if (File.Exists(SubmissionsLogFile))
            {
                string existingJson = File.ReadAllText(SubmissionsLogFile);
                log = JsonUtility.FromJson<SubmissionsLog>(existingJson);
                if (log == null || log.submissions == null)
                {
                    log = new SubmissionsLog { submissions = new List<PollSubmission>() };
                }
            }
            else
            {
                log = new SubmissionsLog { submissions = new List<PollSubmission>() };
            }

            log.submissions.Add(submission);
            log.lastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            log.totalCount = log.submissions.Count;

            string json = JsonUtility.ToJson(log, true);
            File.WriteAllText(SubmissionsLogFile, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[PollBooth] Failed to update submissions log: {e.Message}");
        }
    }

    /// <summary>
    /// Add submission to pending queue (for retry if email fails)
    /// </summary>
    private static void AddToPendingQueue(PollSubmission submission)
    {
        try
        {
            PendingQueue queue;

            if (File.Exists(PendingQueueFile))
            {
                string existingJson = File.ReadAllText(PendingQueueFile);
                queue = JsonUtility.FromJson<PendingQueue>(existingJson);
                if (queue == null || queue.pending == null)
                {
                    queue = new PendingQueue { pending = new List<PollSubmission>() };
                }
            }
            else
            {
                queue = new PendingQueue { pending = new List<PollSubmission>() };
            }

            queue.pending.Add(submission);
            string json = JsonUtility.ToJson(queue, true);
            File.WriteAllText(PendingQueueFile, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[PollBooth] Failed to update pending queue: {e.Message}");
        }
    }

    /// <summary>
    /// Remove a submission from the pending queue (after successful send)
    /// </summary>
    private static void RemoveFromPendingQueue(PollSubmission submission)
    {
        try
        {
            if (!File.Exists(PendingQueueFile)) return;

            string existingJson = File.ReadAllText(PendingQueueFile);
            PendingQueue queue = JsonUtility.FromJson<PendingQueue>(existingJson);

            if (queue != null && queue.pending != null)
            {
                queue.pending.RemoveAll(p => p.timestamp == submission.timestamp && p.subject == submission.subject);
                string json = JsonUtility.ToJson(queue, true);
                File.WriteAllText(PendingQueueFile, json);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[PollBooth] Failed to remove from pending queue: {e.Message}");
        }
    }

    /// <summary>
    /// Attempt to send email (placeholder - requires .NET mail libraries)
    /// In Unity, you would typically use a web service or plugin for this
    /// </summary>
    private static void TrySendEmail(PollSubmission submission)
    {
        // Check if config exists
        if (!File.Exists(EmailConfigPath))
        {
            Debug.LogWarning("[PollBooth] EmailConfig.json not found. Email not sent. Submission saved locally.");
            return;
        }

        try
        {
            string configJson = File.ReadAllText(EmailConfigPath);
            EmailConfig config = JsonUtility.FromJson<EmailConfig>(configJson);

            if (config == null || string.IsNullOrEmpty(config.senderEmail))
            {
                Debug.LogWarning("[PollBooth] Invalid email configuration. Email not sent.");
                return;
            }

            // Format email body
            string emailBody = FormatEmailBody(submission);

            // NOTE: Unity's .NET subset doesn't include System.Net.Mail by default
            // For WebGL builds, you'll need to use a web API instead
            // For standalone builds, you can use System.Net.Mail or a plugin
            //
            // Option 1: Use Unity Web Request to a backend API
            // Option 2: Use a plugin like "Simple Email" from Asset Store
            // Option 3: For standalone, add System.Net.Mail reference
            //
            // Placeholder for email sending:
            Debug.Log($"[PollBooth] EMAIL WOULD BE SENT TO: {config.recipientEmail}");
            Debug.Log($"[PollBooth] Subject: [Fish or Die Feedback] {submission.category}: {submission.subject}");
            Debug.Log($"[PollBooth] Body Preview: {emailBody.Substring(0, Math.Min(200, emailBody.Length))}...");

            // For now, just mark as "would send" - actual implementation depends on target platform
            // RemoveFromPendingQueue(submission); // Uncomment when email actually sends

        }
        catch (Exception e)
        {
            Debug.LogError($"[PollBooth] Email send failed: {e.Message}");
        }
    }

    /// <summary>
    /// Format the email body with all submission details
    /// </summary>
    private static string FormatEmailBody(PollSubmission submission)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("=== FISH OR DIE - PLAYER FEEDBACK ===");
        sb.AppendLine();
        sb.AppendLine($"Category: {submission.category}");
        sb.AppendLine($"Date: {submission.timestamp}");
        sb.AppendLine($"Game Version: {submission.gameVersion}");
        sb.AppendLine($"Player Level: {submission.playerLevel}");
        sb.AppendLine();
        sb.AppendLine("--- PLAYER INFO ---");
        sb.AppendLine($"Name: {submission.playerName}");
        sb.AppendLine($"Email: {(string.IsNullOrEmpty(submission.playerEmail) ? "Not provided" : submission.playerEmail)}");
        sb.AppendLine();
        sb.AppendLine("--- SUBJECT ---");
        sb.AppendLine(submission.subject);
        sb.AppendLine();
        sb.AppendLine("--- MESSAGE ---");
        sb.AppendLine(submission.message);
        sb.AppendLine();
        sb.AppendLine("==========================================");
        sb.AppendLine("This message was sent from the in-game Poll Booth.");

        return sb.ToString();
    }

    /// <summary>
    /// Get count of pending submissions (for debug/UI)
    /// </summary>
    public static int GetPendingCount()
    {
        try
        {
            if (!File.Exists(PendingQueueFile)) return 0;

            string json = File.ReadAllText(PendingQueueFile);
            PendingQueue queue = JsonUtility.FromJson<PendingQueue>(json);
            return queue?.pending?.Count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Get total submissions count
    /// </summary>
    public static int GetTotalSubmissionsCount()
    {
        try
        {
            if (!File.Exists(SubmissionsLogFile)) return 0;

            string json = File.ReadAllText(SubmissionsLogFile);
            SubmissionsLog log = JsonUtility.FromJson<SubmissionsLog>(json);
            return log?.totalCount ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Retry sending all pending submissions
    /// Call this on game startup or periodically
    /// </summary>
    public static void RetryPendingSubmissions()
    {
        if (!USE_EMAIL_SERVICE) return;

        try
        {
            if (!File.Exists(PendingQueueFile)) return;

            string json = File.ReadAllText(PendingQueueFile);
            PendingQueue queue = JsonUtility.FromJson<PendingQueue>(json);

            if (queue?.pending != null && queue.pending.Count > 0)
            {
                Debug.Log($"[PollBooth] Retrying {queue.pending.Count} pending submissions...");

                foreach (var submission in queue.pending)
                {
                    TrySendEmail(submission);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[PollBooth] Failed to retry pending submissions: {e.Message}");
        }
    }

    /// <summary>
    /// Export all submissions to a single file for manual review
    /// </summary>
    public static string ExportAllSubmissions()
    {
        try
        {
            if (!File.Exists(SubmissionsLogFile))
            {
                return "No submissions found.";
            }

            string json = File.ReadAllText(SubmissionsLogFile);
            SubmissionsLog log = JsonUtility.FromJson<SubmissionsLog>(json);

            if (log?.submissions == null || log.submissions.Count == 0)
            {
                return "No submissions found.";
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== FISH OR DIE - ALL SUBMISSIONS EXPORT ===");
            sb.AppendLine($"Export Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Total Submissions: {log.totalCount}");
            sb.AppendLine();

            int index = 1;
            foreach (var submission in log.submissions)
            {
                sb.AppendLine($"--- Submission #{index} ---");
                sb.AppendLine(FormatEmailBody(submission));
                sb.AppendLine();
                index++;
            }

            // Save export file
            string exportPath = Path.Combine(SubmissionsFolder, $"export_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            File.WriteAllText(exportPath, sb.ToString());

            Debug.Log($"[PollBooth] Exported {log.totalCount} submissions to: {exportPath}");
            return exportPath;
        }
        catch (Exception e)
        {
            Debug.LogError($"[PollBooth] Export failed: {e.Message}");
            return $"Export failed: {e.Message}";
        }
    }
}

/// <summary>
/// Email configuration structure
/// Create EmailConfig.json in StreamingAssets folder
/// </summary>
[Serializable]
public class EmailConfig
{
    public string smtpServer = "smtp.gmail.com";
    public int smtpPort = 587;
    public string senderEmail = "";
    public string senderPassword = ""; // Use App Password, not regular password!
    public string recipientEmail = "";
    public bool enableSsl = true;
}

/// <summary>
/// Log of all submissions
/// </summary>
[Serializable]
public class SubmissionsLog
{
    public List<PollSubmission> submissions;
    public string lastUpdated;
    public int totalCount;
}

/// <summary>
/// Queue of submissions pending email send
/// </summary>
[Serializable]
public class PendingQueue
{
    public List<PollSubmission> pending;
}
