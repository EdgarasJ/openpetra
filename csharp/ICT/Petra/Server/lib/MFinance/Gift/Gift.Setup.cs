//
// DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
//
// @Authors:
//       timop
//
// Copyright 2004-2018 by OM International
//
// This file is part of OpenPetra.org.
//
// OpenPetra.org is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// OpenPetra.org is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with OpenPetra.org.  If not, see <http://www.gnu.org/licenses/>.
//
using System;
using System.Collections.Specialized;
using System.Data;
using System.Data.Odbc;
using System.Xml;
using System.IO;
using GNU.Gettext;
using Ict.Common;
using Ict.Common.IO;
using Ict.Common.DB;
using Ict.Common.Verification;
using Ict.Common.Data;
using Ict.Petra.Shared;
using Ict.Petra.Shared.MFinance.Account.Data;
using Ict.Petra.Shared.MFinance.Gift.Data;
using Ict.Petra.Server.MFinance.Gift.Data.Access;
using Ict.Petra.Server.App.Core.Security;
using Ict.Petra.Server.MFinance.Account.Data.Access;

namespace Ict.Petra.Server.MFinance.Gift.WebConnectors
{
    /// <summary>
    /// setup the motivation groups and motivation details
    /// </summary>
    public class TGiftSetupWebConnector
    {
        /// <summary>
        /// returns all motivation groups and details for this ledger
        /// </summary>
        /// <param name="ALedgerNumber"></param>
        /// <param name="AMotivationGroupCode">if this is set, only the details of the specified motivation group will be returned</param>
        /// <returns></returns>
        [RequireModulePermission("FINANCE-1")]
        public static GiftBatchTDS LoadMotivationDetails(Int32 ALedgerNumber, string AMotivationGroupCode)
        {
            GiftBatchTDS MainDS = new GiftBatchTDS();

            TDBTransaction Transaction = null;

            DBAccess.GDBAccessObj.GetNewOrExistingAutoReadTransaction(IsolationLevel.ReadCommitted,
                TEnforceIsolationLevel.eilMinimum,
                ref Transaction,
                delegate
                {
                    ALedgerAccess.LoadByPrimaryKey(MainDS, ALedgerNumber, Transaction);
                    AMotivationGroupAccess.LoadViaALedger(MainDS, ALedgerNumber, Transaction);

                    if (AMotivationGroupCode.Length > 0)
                    {
                        AMotivationDetailAccess.LoadViaAMotivationGroup(MainDS, ALedgerNumber, AMotivationGroupCode, Transaction);
                    }
                    else
                    {
                        AMotivationDetailAccess.LoadViaALedger(MainDS, ALedgerNumber, Transaction);
                    }

                    AMotivationDetailFeeAccess.LoadViaALedger(MainDS, ALedgerNumber, Transaction);
                });

            // Accept row changes here so that the Client gets 'unmodified' rows
            MainDS.AcceptChanges();

            // Remove all Tables that were not filled with data before remoting them.
            MainDS.RemoveEmptyTables();

            return MainDS;
        }

        /// <summary>
        /// maintain motivation group
        /// </summary>
        [RequireModulePermission("FINANCE-3")]
        public static bool MaintainMotivationGroups(string action, Int32 ALedgerNumber,
            String AMotivationGroupCode, String AMotivationGroupDescription, bool AGroupStatus,
            out TVerificationResultCollection AVerificationResult)
        {
            AVerificationResult = new TVerificationResultCollection();
            GiftBatchTDS MainDS = new GiftBatchTDS();

            if (action == "create")
            {
                AMotivationGroupRow row = MainDS.AMotivationGroup.NewRowTyped();
                row.LedgerNumber = ALedgerNumber;
                row.MotivationGroupCode = AMotivationGroupCode.ToUpper();
                row.MotivationGroupDescription = AMotivationGroupDescription;
                row.GroupStatus = AGroupStatus;
                MainDS.AMotivationGroup.Rows.Add(row);

                try
                {
                    GiftBatchTDSAccess.SubmitChanges(MainDS);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else if (action == "update")
            {
                MainDS = LoadMotivationDetails(ALedgerNumber, AMotivationGroupCode);

                foreach (AMotivationGroupRow row in MainDS.AMotivationGroup.Rows)
                {
                    if (row.MotivationGroupCode == AMotivationGroupCode)
                    {
                        row.MotivationGroupDescription = AMotivationGroupDescription;
                        row.GroupStatus = AGroupStatus;
                    }
                }

                try
                {
                    GiftBatchTDSAccess.SubmitChanges(MainDS);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else if (action == "delete")
            {
                MainDS = LoadMotivationDetails(ALedgerNumber, AMotivationGroupCode);

                foreach (AMotivationGroupRow row in MainDS.AMotivationGroup.Rows)
                {
                    if (row.MotivationGroupCode == AMotivationGroupCode)
                    {
                        row.Delete();
                    }
                }

                foreach (AMotivationDetailRow row in MainDS.AMotivationDetail.Rows)
                {
                    if (row.MotivationGroupCode == AMotivationGroupCode)
                    {
                        row.Delete();
                    }
                }

                try
                {
                    GiftBatchTDSAccess.SubmitChanges(MainDS);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// maintain motivation detail
        /// </summary>
        [RequireModulePermission("FINANCE-3")]
        public static bool MaintainMotivationDetails(string action, Int32 ALedgerNumber,
            String AMotivationGroupCode, String AMotivationDetailCode,
            String AMotivationDetailDesc,
            String AAccountCode, String ACostCentreCode, bool AMotivationStatus,
            out TVerificationResultCollection AVerificationResult)
        {
            AVerificationResult = new TVerificationResultCollection();
            GiftBatchTDS MainDS = new GiftBatchTDS();

            if (action == "create")
            {
                AMotivationDetailRow row = MainDS.AMotivationDetail.NewRowTyped();
                row.LedgerNumber = ALedgerNumber;
                row.MotivationGroupCode = AMotivationGroupCode.ToUpper();
                row.MotivationDetailCode = AMotivationDetailCode.ToUpper();
                row.MotivationDetailDesc = AMotivationDetailDesc;
                row.AccountCode = AAccountCode;
                row.CostCentreCode = ACostCentreCode;
                row.MotivationStatus = AMotivationStatus;
                MainDS.AMotivationDetail.Rows.Add(row);

                try
                {
                    GiftBatchTDSAccess.SubmitChanges(MainDS);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else if (action == "update")
            {
                MainDS = LoadMotivationDetails(ALedgerNumber, AMotivationGroupCode);

                foreach (AMotivationDetailRow row in MainDS.AMotivationDetail.Rows)
                {
                    if (row.MotivationGroupCode == AMotivationGroupCode
                        && row.MotivationDetailCode == AMotivationDetailCode)
                    {
                        row.MotivationDetailDesc = AMotivationDetailDesc;
                        row.AccountCode = AAccountCode;
                        row.CostCentreCode = ACostCentreCode;
                        row.MotivationStatus = AMotivationStatus;
                    }
                }

                try
                {
                    GiftBatchTDSAccess.SubmitChanges(MainDS);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else if (action == "delete")
            {
                MainDS = LoadMotivationDetails(ALedgerNumber, AMotivationGroupCode);

                foreach (AMotivationDetailRow row in MainDS.AMotivationDetail.Rows)
                {
                    if (row.MotivationGroupCode == AMotivationGroupCode
                        && row.MotivationDetailCode == AMotivationDetailCode)
                    {
                        row.Delete();
                    }
                }

                try
                {
                    GiftBatchTDSAccess.SubmitChanges(MainDS);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}
