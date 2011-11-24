//
// DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
//
// @Authors:
//       timop
//
// Copyright 2004-2010 by OM International
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
using System.Data;
using System.Windows.Forms;
using System.Collections.Generic;
using GNU.Gettext;
using Ict.Common.Verification;
using Ict.Common;
using Ict.Petra.Client.CommonControls;
using Ict.Petra.Client.App.Core.RemoteObjects;
using Ict.Petra.Client.MFinance.Logic;
using Ict.Petra.Client.MFinance.Gui.GL;
using Ict.Petra.Shared.MFinance.AP.Data;
using Ict.Petra.Shared.MFinance;

namespace Ict.Petra.Client.MFinance.Gui.AP
{
    public partial class TFrmAPEditDocument
    {
    	private void InitializeManualCode()
    	{
    	}
    	
    	private void RunOnceOnActivationManual()
    	{
    		
    	}
    		
    	private void EnableControls()
    	{
    		// I need to make everything read-only if this document was already posted.
    		if ("|POSTED|PARTPAID|PAID|".IndexOf(FMainDS.AApDocument[0].DocumentStatus) > 0)
    		{
    			tbbPostDocument.Enabled = false;
    			
    			txtSupplierName.Enabled = false;
    			txtSupplierCurrency.Enabled = false;
    			txtDocumentCode.Enabled = false;
    			cmbDocumentType.Enabled = false;
    			txtReference.Enabled = false;
    			dtpDateIssued.Enabled = false;
    			dtpDateDue.Enabled = false;
    			nudCreditTerms.Enabled = false;
    			nudDiscountDays.Enabled = false;
    			txtDiscountPercentage.Enabled = false;
				txtTotalAmount.Enabled = false;
				txtExchangeRateToBase.Enabled = false;
				
				btnAddDetail.Enabled = false;
				btnRemoveDetail.Enabled = false;
				btnAnalysisAttributes.Enabled = false;
				
				txtDetailNarrative.Enabled = false;
				txtDetailItemRef.Enabled = false;
				txtDetailAmount.Enabled = false;
				cmbDetailCostCentreCode.Enabled = false;
				btnUseTaxAccount.Enabled = false;
				txtDetailBaseAmount.Enabled = false;
				cmbDetailAccountCode.Enabled = false;
    		}
    		else
    		{
	        	btnRemoveDetail.Enabled = (GetSelectedDetailRow() != null);
    		}
    		
    	}
    		
    	private void ShowDataManual()
    	{
    		txtTotalAmount.CurrencySymbol = FMainDS.AApSupplier[0].CurrencyCode;
    		txtDetailAmount.CurrencySymbol = FMainDS.AApSupplier[0].CurrencyCode;
    		EnableControls();
    	}
    	
        private void NewDetail(Object sender, EventArgs e)
        {
            // get the entered amounts, so that we can calculate the missing amount for the new detail
            GetDetailsFromControls(FPreviouslySelectedDetailRow);

            decimal DetailAmount = FMainDS.AApDocument[0].TotalAmount;

            if (FMainDS.AApDocumentDetail != null)
            {
                foreach (AApDocumentDetailRow detailRow in FMainDS.AApDocumentDetail.Rows)
                {
                	if (detailRow.RowState != DataRowState.Deleted)
                	{
                    	DetailAmount -= detailRow.Amount;
                	}
                }
            }

            if (DetailAmount < 0)
            {
                DetailAmount = 0;
            }

            CreateAApDocumentDetail(
                FMainDS.AApDocument[0].LedgerNumber,
                FMainDS.AApDocument[0].ApNumber,
                FMainDS.AApSupplier[0].DefaultExpAccount,
                FMainDS.AApSupplier[0].DefaultCostCentre,
                DetailAmount,
                FMainDS.AApDocument[0].LastDetailNumber);
            FMainDS.AApDocument[0].LastDetailNumber++;

            // for the moment, set all to approved, since we don't yet support approval of documents
            FMainDS.AApDocument[0].DocumentStatus = MFinanceConstants.AP_DOCUMENT_APPROVED;
    		EnableControls();
        	txtDetailNarrative.Focus();
        }

        private void RemoveDetail(Object sender, EventArgs e)
        {
        	AApDocumentDetailRow Row = GetSelectedDetailRow();
        	if (Row == null)
        		return;
        	
		    GetDataFromControls(FMainDS.AApDocument[0]);
            int rowIndex = grdDetails.Selection.GetSelectionRegion().GetRowsIndex()[0];
            MessageBox.Show("Deleting "+ Row.Narrative, "Remove Row");
            
		    Row.Delete();  // This row is not removed, but marked for deletion.
		    
		    // I have to prevent the auto-generated code from attempting to access this deleted row.
		    grdDetails.Selection.SelectRow(rowIndex, true);
		    FPreviouslySelectedDetailRow = GetSelectedDetailRow();
		    ShowDetails(FPreviouslySelectedDetailRow);
		    // Then I need to re-draw the grid, and enable controls as appropriate.
		    grdDetails.Refresh();
	        FPetraUtilsObject.SetChangedFlag();
    		EnableControls();
        }
        
        private void Analyse (Object sender, EventArgs e)
        	
        {
        	
        }
        
        private void UseTaxAccount(Object sender, EventArgs e)
        	
        {
        	
        }
        
        private void UpdateCreditTerms(object sender, TPetraDateChangedEventArgs e)
        {
            if (sender == dtpDateDue)
            {
                if ((dtpDateDue.Date.HasValue)
                    && (dtpDateIssued.Date.HasValue))
                {
                    int diffDays = (dtpDateDue.Date.Value - dtpDateIssued.Date.Value).Days;

                    if (diffDays < 0)
                    {
                        diffDays = 0;
                        dtpDateDue.Date = dtpDateIssued.Date.Value;
                    }

                    nudCreditTerms.Value = diffDays;
                }
            }
            else if ((sender == dtpDateIssued) || (sender == nudCreditTerms))
            {
                if ((dtpDateIssued.Date.HasValue))
                {
                    dtpDateDue.Date = dtpDateIssued.Date.Value.AddDays((double)nudCreditTerms.Value);
                }
            }
        }

        private void UpdateCreditTermsOverload(object sender, EventArgs e)
        {
            UpdateCreditTerms(sender, null);
        }

        /// initialise some comboboxes
        private void BeforeShowDetailsManual(AApDocumentDetailRow ARow)
        {
            // if this form is readonly, then we need all account and cost centre codes, because old codes might have been used
            bool ActiveOnly = this.Enabled;

            TFinanceControls.InitialiseAccountList(ref cmbDetailAccountCode, ARow.LedgerNumber, true, false, ActiveOnly, false);
            TFinanceControls.InitialiseCostCentreList(ref cmbDetailCostCentreCode, ARow.LedgerNumber, true, false, ActiveOnly, false);
    		EnableControls();
        	
        	if (ARow.IsAmountNull() || (FMainDS.AApDocument[0].IsExchangeRateToBaseNull() || FMainDS.AApDocument[0].ExchangeRateToBase == 0))
        	{
	            txtDetailBaseAmount.NumberValueDecimal = null;
        	}
        	else
        	{
	        	decimal DetailAmount = Convert.ToDecimal(ARow.Amount);
	        	DetailAmount *= FMainDS.AApDocument[0].ExchangeRateToBase;
	            txtDetailBaseAmount.NumberValueDecimal = DetailAmount;
        	}
        	
        }
        
        private bool BatchBalancesOK()
        {
        	decimal DocumentBalance = FMainDS.AApDocument[0].TotalAmount;
        	
        	foreach (AApDocumentDetailRow Row in FMainDS.AApDocumentDetail.Rows)
        	{
        		DocumentBalance -= Row.Amount;
        	}
        	if (DocumentBalance == 0.0m)
        	{
        		return true;
        	}
        	else
        	{
         		System.Windows.Forms.MessageBox.Show(Catalog.GetString("The document Amount does not equal the sum of the detail lines."), Catalog.GetString("Balance Problem"));
	         	return false;
        	}
        }

        /// <summary>
        /// Post document as a GL Batch
        /// see very similar function in TFrmAPSupplierTransactions
        /// </summary>
        private void PostDocument(object sender, EventArgs e)
        {
            List <Int32>TaggedDocuments = new List <Int32>();

            TaggedDocuments.Add(FMainDS.AApDocument[0].ApNumber);

            if (TaggedDocuments.Count == 0)
            {
                return;
            }

            // TODO: make sure that there are uptodate exchange rates
            
            // If the batch will not balance, I'll stop right here..
            if (!BatchBalancesOK())
            	return;

            TVerificationResultCollection Verifications;

            TDlgGLEnterDateEffective dateEffectiveDialog = new TDlgGLEnterDateEffective(
                FMainDS.AApDocument[0].LedgerNumber,
                Catalog.GetString("Select posting date"),
                Catalog.GetString("The date effective for posting") + ":");

            if (dateEffectiveDialog.ShowDialog() != DialogResult.OK)
            {
                MessageBox.Show(Catalog.GetString("The payment was cancelled."), Catalog.GetString(
                        "No Success"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DateTime PostingDate = dateEffectiveDialog.SelectedDate;

            if (!TRemote.MFinance.AP.WebConnectors.PostAPDocuments(
                    FMainDS.AApDocument[0].LedgerNumber,
                    TaggedDocuments,
                    PostingDate,
                    out Verifications))
            {
                string ErrorMessages = String.Empty;

                foreach (TVerificationResult verif in Verifications)
                {
                    ErrorMessages += "[" + verif.ResultContext + "] " +
                                     verif.ResultTextCaption + ": " +
                                     verif.ResultText + Environment.NewLine;
                }

                System.Windows.Forms.MessageBox.Show(ErrorMessages, Catalog.GetString("Posting failed"));
            }
            else
            {
                // TODO: print reports on successfully posted batch
                MessageBox.Show(Catalog.GetString("The AP document has been posted successfully!"));

                // TODO: show posting register of GL Batch?

                // TODO: refresh the screen, to reflect that the transactions have been posted

                Form Opener = FPetraUtilsObject.GetCallerForm();
                if (Opener.GetType() == typeof(TFrmAPSupplierTransactions))
                {
                	((TFrmAPSupplierTransactions)Opener).Reload();
                }
            }
        }
    }
}