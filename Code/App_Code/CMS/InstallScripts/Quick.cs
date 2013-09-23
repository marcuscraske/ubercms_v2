﻿using CMS.Base;
using System;
using System.Text;
using UberLib.Connector;

namespace CMS.InstallScripts
{
    public class Quick : Base
    {
        public override bool install(ref StringBuilder messageOutput)
        {
            // Start only the absolute base of the CMS
            Core.start(true);
            // Check the base has started
            if (Core.State == Core.CoreState.Failed)
            {
                messageOutput.Append("Absolute base of CMS started with failed-state, cannot continue! Error-message: '").Append(Core.ErrorMessage ?? "(no error message)").AppendLine("'.");
                return false;
            }
            // Load settings from disk
            Settings settings = Settings.loadFromDisk(Core.CmsConfigPath);
            // Create connector
            Connector conn = Core.connectorCreate(true, ref settings);
            // Install CMS database
            if (!BaseUtils.executeSQL(Core.BasePath + "/installer/sql/mysql/install.sql", conn, ref messageOutput))
            {
                messageOutput.AppendLine("Failed to install base SQL!");
                return false;
            }
            // Restart core to load as usual
            Core.stop();
            Core.start();
            // Check the core has started
            if (Core.State != Core.CoreState.Started)
            {
                messageOutput.Append("Failed to start core after SQL installation (state: ").Append(Core.State.ToString()).Append("): ").Append(Core.ErrorMessage ?? "(no error message)").Append(".");
                return false;
            }
            // Install core templates
            if (!Core.Templates.install(conn, null, Core.BasePath + "/installer/templates", ref messageOutput))
            {
                messageOutput.AppendLine("Failed to install core templates!");
                return false;
            }
            // Install package developer
            Plugin p = null;
            if (Core.Plugins.createFromDirectory(conn, Core.BasePath + "/App_Code/CMS/Plugins/Package Developer", ref p, ref messageOutput) && p != null)
            {
                Core.Plugins.install(conn, p, ref messageOutput);
                Core.Plugins.enable(conn, p, ref messageOutput);
            }
            // Output status
            messageOutput.AppendLine("Successfully installed CMS!");
            return true;
        }
        public override string Title
        {
            get
            {
                return "Quick Installation - Basic Development Kit - MySQL";
            }
        }
        public override CMS.Base.Version Version
        {
            get
            {
                return new CMS.Base.Version(1, 0, 0);
            }
        }
        public override string Author
        {
            get
            {
                return "limpygnome <limpygnome@gmail.com>";
            }
        }
    }
}