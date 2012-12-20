using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace IronSharePoint.EventReceivers
{
    public class IronItemEventReceiver: SPItemEventReceiver
    {
        public static void Register(SPContentType ct, SPEventReceiverType eventType, SPEventReceiverSynchronization synchronization, int sequenceNumber, string className)
        {
            Register(ct, eventType, synchronization, sequenceNumber, className, true);
        }

        public static void Register(SPContentType ct, SPEventReceiverType eventType, SPEventReceiverSynchronization synchronization, int sequenceNumber, string className, bool updateChildren)
        {
            if (!IsRegistered(ct, eventType, className))
            {
                ct.ParentWeb.AllowUnsafeUpdates = true;
                var assemblyName = typeof(IronItemEventReceiver).Assembly.FullName;
                var typeName = typeof(IronItemEventReceiver).FullName;

                var receiver = ct.EventReceivers.Add();
                receiver.Type = eventType;
                receiver.Assembly = assemblyName;
                receiver.Synchronization = synchronization;
                receiver.Class = typeName;
                receiver.Name = className;
                receiver.Data = className;
                receiver.SequenceNumber = sequenceNumber;

                receiver.Update();
                ct.Update(updateChildren);

                ct.ParentWeb.AllowUnsafeUpdates = false;
            }
        }

        public static void Unregister(SPContentType ct, SPEventReceiverType eventType, string className)
        {
            Unregister(ct, eventType, className, true);
        }

        public static void Unregister(SPContentType ct, SPEventReceiverType eventType, string className, bool updateChildren)
        {
            var receiver = ct.EventReceivers.OfType<SPEventReceiverDefinition>().FirstOrDefault(e => e.Class == className && (e.Type & eventType) == eventType);
            if (receiver != null)
            {
                receiver.Delete();
                ct.Update(updateChildren);
            }
        }

        public static bool IsRegistered(SPContentType ct, SPEventReceiverType eventType, string className)
        {
            return ct.EventReceivers.OfType<SPEventReceiverDefinition>().Any(e => e.Class == className && (e.Type & eventType) == eventType);
        }

        public static void Register(SPList list, SPEventReceiverType eventType, SPEventReceiverSynchronization synchronization, int sequenceNumber, string className)
        {

            if (!IsRegistered(list, eventType, className))
            {
                list.ParentWeb.AllowUnsafeUpdates = true;
                var assemblyName = typeof(IronItemEventReceiver).Assembly.FullName;
                var typeName = typeof(IronItemEventReceiver).FullName;

                var receiver = list.EventReceivers.Add();
                receiver.Type = eventType;
                receiver.Assembly = assemblyName;
                receiver.Synchronization = synchronization;
                receiver.HostType = SPEventHostType.List;
                receiver.HostId = list.ID;
                receiver.Class = typeName;
                receiver.Name = className;
                receiver.Data = className;
                receiver.SequenceNumber = sequenceNumber;

                receiver.Update();
                list.Update();

                list.ParentWeb.AllowUnsafeUpdates = false;
            }
        }

        public static void Unregister(SPList list, SPEventReceiverType eventType, string className)
        {
            var receiver = list.EventReceivers.OfType<SPEventReceiverDefinition>().FirstOrDefault(e => e.Class == className && (e.Type & eventType)==eventType);
            if (receiver != null) receiver.Delete();
        }

        public static bool IsRegistered(SPList list, SPEventReceiverType eventType, string className)
        {
            return list.EventReceivers.OfType<SPEventReceiverDefinition>().Any(e => e.Class == className && (e.Type & eventType)==eventType);
        }

        public static List<SPEventReceiverDefinition> GetAllRegistered(SPList list)
        {
            var typeName = typeof(IronItemEventReceiver).FullName;

            return list.EventReceivers.OfType<SPEventReceiverDefinition>().Where(e => e.Class == typeName).ToList();
        }

        private void CallDynamicEventreceiver(SPItemEventProperties properties)
        {
            var dynEventReceiver = IronRuntime.GetDefaultIronRuntime(properties.Web.Site).CreateDynamicInstance(properties.ReceiverData) as SPItemEventReceiver;
            
            switch (properties.EventType)
            {
                case SPEventReceiverType.ItemAdded: dynEventReceiver.ItemAdded(properties); break;
                case SPEventReceiverType.ItemUpdated: dynEventReceiver.ItemUpdated(properties); break;
                case SPEventReceiverType.ItemDeleted: dynEventReceiver.ItemDeleted(properties); break;
                case SPEventReceiverType.ItemFileMoved: dynEventReceiver.ItemFileMoved(properties); break;
                case SPEventReceiverType.ItemCheckedIn: dynEventReceiver.ItemCheckedIn(properties); break;
                case SPEventReceiverType.ItemCheckedOut: dynEventReceiver.ItemCheckedOut(properties); break;
                case SPEventReceiverType.ItemAdding: dynEventReceiver.ItemAdding(properties); break;
                case SPEventReceiverType.ItemUpdating: dynEventReceiver.ItemUpdating(properties); break;
                case SPEventReceiverType.ItemDeleting: dynEventReceiver.ItemDeleting(properties); break;
                case SPEventReceiverType.ItemFileMoving: dynEventReceiver.ItemFileMoving(properties); break;
                case SPEventReceiverType.ItemCheckingIn: dynEventReceiver.ItemCheckingIn(properties); break;
                case SPEventReceiverType.ItemCheckingOut: dynEventReceiver.ItemCheckingOut(properties); break;
                case SPEventReceiverType.ItemUncheckedOut: dynEventReceiver.ItemUncheckedOut(properties); break;
                case SPEventReceiverType.ItemUncheckingOut: dynEventReceiver.ItemUncheckingOut(properties); break;
                case SPEventReceiverType.ItemAttachmentAdded: dynEventReceiver.ItemAttachmentAdded(properties); break;
                case SPEventReceiverType.ItemAttachmentAdding: dynEventReceiver.ItemAttachmentAdding(properties); break;
                case SPEventReceiverType.ItemAttachmentDeleted: dynEventReceiver.ItemAttachmentDeleted(properties); break;
                case SPEventReceiverType.ItemAttachmentDeleting: dynEventReceiver.ItemAttachmentDeleting(properties); break;
                case SPEventReceiverType.ItemFileConverted: dynEventReceiver.ItemFileConverted(properties); break;
            }
            
        }

        public override void ItemAdded(SPItemEventProperties properties)
        {
            CallDynamicEventreceiver(properties);
        }

        public override void ItemUpdated(SPItemEventProperties properties)
        {
            CallDynamicEventreceiver(properties);
        }

        public override void ItemDeleted(SPItemEventProperties properties)
        {
            CallDynamicEventreceiver(properties);
        }

        public override void ItemFileMoved(SPItemEventProperties properties)
        {
            CallDynamicEventreceiver(properties);
        }


        public override void ItemCheckedIn(SPItemEventProperties properties)
        {
            CallDynamicEventreceiver(properties);
        }

        public override void ItemCheckedOut(SPItemEventProperties properties)
        {
            CallDynamicEventreceiver(properties);
        }

        public override void ItemAdding(SPItemEventProperties properties)
        {
            CallDynamicEventreceiver(properties);
        }

        public override void ItemUpdating(SPItemEventProperties properties)
        {
            CallDynamicEventreceiver(properties);
        }

        public override void ItemDeleting(SPItemEventProperties properties)
        {
            CallDynamicEventreceiver(properties);
        }

        public override void ItemFileMoving(SPItemEventProperties properties)
        {
            CallDynamicEventreceiver(properties);
        }

        public override void ItemCheckingIn(SPItemEventProperties properties)
        {
            CallDynamicEventreceiver(properties);
        }

        public override void ItemCheckingOut(SPItemEventProperties properties)
        {
            CallDynamicEventreceiver(properties);
        }

        public override void ItemAttachmentAdded(SPItemEventProperties properties)
        {
            CallDynamicEventreceiver(properties);
        }

        public override void ItemAttachmentAdding(SPItemEventProperties properties)
        {
            CallDynamicEventreceiver(properties);
        }

        public override void ItemAttachmentDeleted(SPItemEventProperties properties)
        {
            CallDynamicEventreceiver(properties);
        }

        public override void ItemAttachmentDeleting(SPItemEventProperties properties)
        {
            CallDynamicEventreceiver(properties);
        }

        public override void ItemFileConverted(SPItemEventProperties properties)
        {
            CallDynamicEventreceiver(properties);
        }

        public override void ItemUncheckedOut(SPItemEventProperties properties)
        {
            CallDynamicEventreceiver(properties);
        }

        public override void ItemUncheckingOut(SPItemEventProperties properties)
        {
            CallDynamicEventreceiver(properties);
        }
    }
}
