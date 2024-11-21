using UI.Dialogs;
using UnityEngine;

namespace UI
{
    public class DialogController : MonoBehaviour
    {
        public RectTransform DialogContainer;
        
        public uDialog ShowSimpleDialog(string title, string text, bool closeButton = false, eIconType iconType = eIconType.None)
        {
            var dialog = uDialog.NewDialog()
                .SetTitleText(title)
                .SetContentText(text)
                .SetIcon(iconType)
                .SetContentFontSize(18)
                .SetDimensions(384, 222)
                .SetModal(true, true)
                .SetDestroyAfterClose(true)
                .SetAllowDraggingViaTitle()
                .SetParent(DialogContainer);

            if (closeButton)
            {
                dialog.AddButton("Close", (d) => d.Close());
            }

            return dialog;
        }
    }
}