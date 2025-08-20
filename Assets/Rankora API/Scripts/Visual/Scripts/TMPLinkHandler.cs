using Rankora_API.Scripts.Rankora.Types;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Rankora_API.Examples.Visual.Scripts
{
    [RequireComponent(typeof(TMP_Text))]
    public class TMPLinkHandler : MonoBehaviour, IPointerClickHandler, IPointerMoveHandler
    {
        [Serializable]
        public class Link
        {
            public string id; public string url;
        }
        public List<Link> links;

        private TMP_Text textMeshPro;
        private string originalText;
        private int currentLinkIndex = -1;

        void Awake()
        {
            textMeshPro = GetComponent<TMP_Text>();
            originalText = textMeshPro.text;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(textMeshPro, Input.mousePosition, null);
            if (linkIndex != -1)
            {
                TMP_LinkInfo linkInfo = textMeshPro.textInfo.linkInfo[linkIndex];
                string linkId = linkInfo.GetLinkID();

                if (linkId.StartsWith("http"))
                {
                    Application.OpenURL(linkId);
                    return;
                }
#if UNITY_EDITOR
                if(linkId == "open.settings")
                {
                    UnityEditor.Selection.activeObject = RankoraSettings.Instance;
                }
#endif

                var link = links.Find(x  => x.id == linkId);
                if (link != null)
                {
                    Application.OpenURL(link.url);
                }
                else
                {
                    Debug.LogWarning("Link " + linkId + " not found in the links.");
                }
            }
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(textMeshPro, Input.mousePosition, null);

            if (linkIndex != currentLinkIndex)
            {
                currentLinkIndex = linkIndex;

                if (currentLinkIndex == -1)
                {
                    // No link hovered, restore original text
                    textMeshPro.text = originalText;
                }
                else
                {
                    // Get the link info
                    TMP_LinkInfo linkInfo = textMeshPro.textInfo.linkInfo[currentLinkIndex];
                    string linkId = linkInfo.GetLinkID();

                    // Find the specific link tag in the original text
                    string linkPattern = $"<link=\"{linkId}\">";
                    int linkStartIndex = originalText.IndexOf(linkPattern, StringComparison.OrdinalIgnoreCase);

                    if (linkStartIndex != -1)
                    {
                        // Find the corresponding closing tag
                        int linkEndIndex = originalText.IndexOf("</link>", linkStartIndex + linkPattern.Length, StringComparison.OrdinalIgnoreCase);

                        if (linkEndIndex != -1)
                        {
                            var sb = new StringBuilder(originalText);

                            // Look for color tag inside the link
                            int searchStart = linkStartIndex + linkPattern.Length;
                            int colorStartIndex = originalText.IndexOf("<color=", searchStart, StringComparison.OrdinalIgnoreCase);
                            int colorEndIndex = -1;

                            // Check if color tag exists within this link
                            if (colorStartIndex != -1 && colorStartIndex < linkEndIndex)
                            {
                                // Find the end of the opening color tag
                                int colorTagEndIndex = originalText.IndexOf(">", colorStartIndex);
                                // Find the closing color tag
                                colorEndIndex = originalText.IndexOf("</color>", colorTagEndIndex, StringComparison.OrdinalIgnoreCase);

                                if (colorTagEndIndex != -1 && colorEndIndex != -1 && colorEndIndex < linkEndIndex)
                                {
                                    // Insert underline tags inside the color tags
                                    // Insert closing </u> first (before </color>)
                                    sb.Insert(colorEndIndex, "</u>");
                                    // Insert opening <u> after the color opening tag
                                    sb.Insert(colorTagEndIndex + 1, "<u>");
                                }
                                else
                                {
                                    // Fallback: insert around the entire link content
                                    sb.Insert(linkEndIndex, "</u>");
                                    sb.Insert(linkStartIndex + linkPattern.Length, "<u>");
                                }
                            }
                            else
                            {
                                // No color tag found, insert around the entire link content
                                sb.Insert(linkEndIndex, "</u>");
                                sb.Insert(linkStartIndex + linkPattern.Length, "<u>");
                            }

                            string newText = sb.ToString();
                            textMeshPro.text = newText;
                        }
                        else
                        {
                            Debug.LogWarning($"Closing </link> tag not found for link: {linkId}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Link tag not found in original text for link: {linkId}");
                    }
                }
            }
        }

    }
}
