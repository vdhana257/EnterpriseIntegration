   M S B A M L   `   `   `  ����G  CTpmMigration, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null�Mclr-namespace:Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration?Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration  5      `    ZMicrosoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.AgreementMigrationPageView   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      ftpmMigrationMclr-namespace:Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration     5        � 	Resources  5	      m  gMicrosoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.MigrationStatusEnumToImagePathConverter +  'migrationStatusEnumToImagePathConvertera  [Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.BoolToOppositeBoolConverter  boolToOppositeBoolConverter_  YMicrosoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.NullToVisibilityConverter  nullToVisibilityConverter��%W   &	        &	      &	 &       5
   
   6?     5   
   63     5   
   61   5      .��5      � -   5        $��Layout��5      $
��Left=�6   $	��Top=�64    � RowDefinitions 5   
   �� 5      $��50 �6   �� 5      $
��Auto �6   .+�5   
   �� $��0��6   All Agreements can be found �� 5   )   -   5         ��Name$ FileLink��5   3    �� Command 6E   �� *OpenAgreementDirCommand+.�6q   here6w   5      &   System.Windows.Controls.DataGrid  5   
   -   5        $ ��AgreementSelectionDataGrid��5      $	��700��65     StandardDataGrid#��[ 5      $��1��5      ��5      �� *MigrationItems+ �� Mode$ OneWay=�   Columns 5      0  *System.Windows.Controls.DataGridTextColumn  5      ,  &System.Windows.Controls.DataGridColumn   CanUserReorder .    CanUserResize .    CanUserSort . 	   
IsReadOnly	 . 
   Width5  /System.Windows.Controls.DataGridLengthConverter$	
 90* 5!   5   X   RMicrosoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.DataGridExtensions   
HeaderName 5   %   �� *AgreementNameColumnHeader+  	Resources �� Source# [ 1	  +System.Windows.Controls.DataGridBoundColumn 	  Binding 5   5   �� *Name+5!   5     5#       .  .  . 	 . $	
 60* 5&   5    5#   %   �� *ProtocolColumnHeader+# [  5$   5   �� *MigrationEntity.Protocol+5&   5     5(       .  .  . 	 . $	
 90* 5+   5    5(   %   �� *HostedPartnerColumnHeader+# [  5)   5   �� *HostedPartnerName+5+   5     5-       .  .  . 	 . $	
 90* 50   5    5-   %   �� *GuestPartnerColumnHeader+# [  5.   5   �� *GuestPartnerName+50   5   4
  .System.Windows.Controls.DataGridTemplateColumn
  52      	 . $
 70 6;    6F   �� *MigrationStatusColumnHeader+# [  
  CellTemplate 53      �� 54      .R�55      �� $��15��6�   ��6$   �� *ImportStatus+ �� 	Converter# [   � ToolTip 56   "   _� 57   &   $	��700��6�   y�6.   �� *ImportStatusText+# [ .��58   *   �� $
��Wrap=�6V   ��64   �� *ImportStatusText+6V   59   '   5;      5<      5>      5@      5A      5B      