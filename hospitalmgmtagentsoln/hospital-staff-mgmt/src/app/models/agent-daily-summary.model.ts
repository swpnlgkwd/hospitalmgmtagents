export interface QuickReply {
  label: string;   // e.g., "📅 Review Shift Coverage"
  value: string;   // e.g., "show uncovered shifts"
}

export interface AgentSummaryResponse {
  summaryMessage: string;         // The main agent message shown to the user
  quickReplies?: QuickReply[];    // Optional list of actionable quick reply buttons
}
