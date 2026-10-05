05/08/2025, 1.3.1:
 - Implemented method to register a delegate directly via code in ScriptableEvents.

04/08/2025, 1.3.0:
 - Implemented channel multiplexing for scriptable events.

07/08/2025, 1.2.0:
 - Added variable reset before build to avoid scenarios where scriptable variables are packaged with wrong value.

07/06/2025, 1.1.6:
 - Added debug logs to RuntimeSets and Lists.

07/04/2025, 1.1.5:
 - Added a per-variable option to use debug logs when the variable is reset or changed.

07/04/2025, 1.1.4:
 - Added a per-event option to use debug logs when raising or listening for an event.

06/19/2025, 1.1.3:
 - Added method to manually reset variables.

05/28/2025, 1.1.2:
 - Fixed persistency issue with Vector data type.
 - Added Quaternion data type.

04/17/2025, 1.1.1:
 - Fixed persistent variables changing UUID across session restart.

04/16/2025, 1.1.0:
 - Fixed packaging yielding errors on build

03/24/2025, 1.0.0:
 - First public release