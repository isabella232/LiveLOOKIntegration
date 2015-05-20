using System;
using System.Windows.Forms;
using ININ.Diagnostics;
using ININ.IceLib.Connection;
using ININ.InteractionClient;
using ININ.InteractionClient.AddIn;
using ININ.InteractionClient.Interactions;

namespace ININ.Alliances.LiveLOOKAddin
{
    public class LiveLookAddin : IAddIn
    {
        public static readonly ITopicTracer AddinTracer = TopicTracerFactory.CreateTopicTracer("ININ.Alliances.LiveLOOKAddin");

        public void Load(IServiceProvider serviceProvider)
        {
            using (LiveLookAddin.AddinTracer.Scope("LiveLookAddin.Load(...)"))
            {
                try
                {
                    var session = serviceProvider != null
                        ? serviceProvider.GetService(typeof (Session)) as Session
                        : null;

                    if (session == null)
                    {
                        throw new Exception("Failed to get session!");
                    }

                    // Add LiveLOOK button
                    AddinTracer.Always("Getting IClientInteractionButtonService...");
                    var service = ServiceLocator.Current.GetInstance<IClientInteractionButtonService>();
                    AddinTracer.Always("IClientInteractionButtonService=" + service);
                    if (service == null)
                        throw new Exception("Unable to locate IClientInteractionButtonService service.");
                    AddinTracer.Always("About to add button...");
                    service.Add(new LiveLookButton(session));
                    AddinTracer.Always("LiveLookButton added");
                }
                catch (Exception ex)
                {
                    LiveLookAddin.AddinTracer.Exception(ex);
                    MessageBox.Show(
                        "Error on load: " + ex.Message + Environment.NewLine + Environment.NewLine +
                        "Please restart the Interaction Client and contact your system administrator if this issue persists.",
                        "Error loading LiveLOOK Addin", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public void Unload()
        {
            
        }
    }
}
