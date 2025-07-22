export interface SmartSuggestion {
  message: string;
  actionPayload: string;
  type: string;

  // Optional UI Action Button Fields
  actionName?: string;   // Identifier for what to trigger (e.g., 'autoAssignUncoveredShifts')
  actionLabel?: string;  // Tooltip text or screen-reader friendly label
  actionIcon?: string;   // Optional icon (e.g., '⚡', '✓', '🛠️')
  actionData?: any;      // Additional data for the action (like shift IDs, staff IDs, etc.)
  actionText? : string; // Text to display in the action button
}
