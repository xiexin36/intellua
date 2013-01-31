Following modifications was made to Scintilla and ScintillaNET, as a result Intellua is not compatible with unmodifed versions:
Scintillua:   Notification SCN_AUTOCMOVED (2028) will be sent when user highlighted a different autocomplete item.
  Showing a calltip will not cancel autocomplete, and vice versa.ScintillaNET:
  Event AutoCompleteCancelled for the SCN_AUTOCCANCELLED notification
  Event AutoCompleteMoved for the SCN_AUTOCMOVED notification