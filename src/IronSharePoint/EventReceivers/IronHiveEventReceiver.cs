using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace IronSharePoint.EventReceivers
{
    public class IronHiveEventReceiver : SPItemEventReceiver
    {

        public new bool EventFiringEnabled
        {
            get { return base.EventFiringEnabled; }
            set { base.EventFiringEnabled = value; }
        }

        public override void ItemAdded(SPItemEventProperties properties)
        {
            IronRuntime.GetDefaultIronRuntime(properties.Web.Site).IronHive.FireHiveEvent(this, "ItemAdded", properties);
        }

        public override void ItemUpdated(SPItemEventProperties properties)
        {
            IronRuntime.GetDefaultIronRuntime(properties.Web.Site).IronHive.FireHiveEvent(this, "ItemUpdated", properties);
        }

        public override void ItemDeleted(SPItemEventProperties properties)
        {
            IronRuntime.GetDefaultIronRuntime(properties.Web.Site).IronHive.FireHiveEvent(this, "ItemDeleted", properties);
        }

        public override void ItemFileMoved(SPItemEventProperties properties)
        {
            IronRuntime.GetDefaultIronRuntime(properties.Web.Site).IronHive.FireHiveEvent(this, "ItemDeleted", properties);
        }


        public override void ItemCheckedIn(SPItemEventProperties properties)
        {
            IronRuntime.GetDefaultIronRuntime(properties.Web.Site).IronHive.FireHiveEvent(this, "ItemCheckedIn", properties);
        }

        public override void ItemCheckedOut(SPItemEventProperties properties)
        {
            IronRuntime.GetDefaultIronRuntime(properties.Web.Site).IronHive.FireHiveEvent(this, "ItemCheckedOut", properties);
        }

        public override void ItemAdding(SPItemEventProperties properties)
        {
            IronRuntime.GetDefaultIronRuntime(properties.Web.Site).IronHive.FireHiveEvent(this, "ItemAdding", properties);
        }

        public override void ItemUpdating(SPItemEventProperties properties)
        {
            IronRuntime.GetDefaultIronRuntime(properties.Web.Site).IronHive.FireHiveEvent(this, "ItemUpdating", properties);
        }

        public override void ItemDeleting(SPItemEventProperties properties)
        {
            IronRuntime.GetDefaultIronRuntime(properties.Web.Site).IronHive.FireHiveEvent(this, "ItemDeleting", properties);
        }

        public override void ItemFileMoving(SPItemEventProperties properties)
        {
            IronRuntime.GetDefaultIronRuntime(properties.Web.Site).IronHive.FireHiveEvent(this, "ItemFileMoving", properties);
        }

        public override void ItemCheckingIn(SPItemEventProperties properties)
        {
            IronRuntime.GetDefaultIronRuntime(properties.Web.Site).IronHive.FireHiveEvent(this, "ItemCheckingIn", properties);
        }

        public override void ItemCheckingOut(SPItemEventProperties properties)
        {
            IronRuntime.GetDefaultIronRuntime(properties.Web.Site).IronHive.FireHiveEvent(this, "ItemCheckingOut", properties);
        }

        public override void ItemUncheckedOut(SPItemEventProperties properties)
        {
            IronRuntime.GetDefaultIronRuntime(properties.Web.Site).IronHive.FireHiveEvent(this, "ItemUncheckedOut", properties);
        }

        public override void ItemUncheckingOut(SPItemEventProperties properties)
        {
            IronRuntime.GetDefaultIronRuntime(properties.Web.Site).IronHive.FireHiveEvent(this, "ItemUncheckingOut", properties);
        }

    }
}
