   M S B A M L   `   `   `  ����G  CTpmMigration, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null�Mclr-namespace:Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration?Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration  5      h    bMicrosoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.ExportToIntegrationAccountPageView   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      ftpmMigrationMclr-namespace:Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration     5        � 	Resources  5	      _  YMicrosoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.NullToVisibilityConverter   nullToVisibilityConverter��%   &	          5
   
   6U   5      .��5      �  � ColumnDefinitions 5   
   �� 5      $��* �6   .+�5   
   �� -   5        $!��CertificateMigrationWarning��5      $��0��5      
���  ��$��0,10,-0.2,434.2q�65   $
��Wrap=�6O   $��30��6c   ��5   7   ��  �� Path$ MigrationWarningText�y�6b   �� *!CertificateMigrationWarningText+ �� 	Converter# [  5   c   �� 5   
   $8��2Select Artifacts to Migrate to Integration Account��6   $
��Left=�6P   $	��Top=�6l   $��	10,30,0,0q�6�   $	��350��6�   $	��320��6�   $��0��6�   .��5      � $
��Left=�6   $	��Top=�6/   .+�5      �� -   5         ��Name$ SchemaExportCheckBox��5      $��	Schema(s)��69   $
��Left=�6M   $��	20,39,0,0q�6h   $	��Top=�6{   {�6�   �� *IsSchemaExportEnabled+ d� 	IsChecked 6�   �� *IsSchemaSelected+ �� Mode$ TwoWay=� �� UpdateSourceTrigger$ PropertyChanged=� � ToolTip 6  �� *SchemasToExport+�� 5      $
��Left=�6   $��
150,40,0,0q�64   $	��Top=�6H   ��6`   �� *SchemaLabelContent+�� 5      -   5        $ CertExportCheckBox��5      $��Certificate(s)��67   $
��Left=�6P   $��	20,91,0,0q�6k   $	��Top=�6~   {�6�   �� *IsCertExportEnabled+ 6�   �� *IsCertSelected+$ TwoWay=�$ PropertyChanged=� 6  �� *CertificatesToExport+�� 5      $
��Left=�6   $��
155,92,0,0q�64   $	��Top=�6H   ��6`   �� *CertLabelContent+�� 5      -   5        $ PartnerExportCheckBox��5      $��
Partner(s)��6:   $
��Left=�6O   $��
20,137,0,0q�6j   $	��Top=�6~   {�6�   �� *IsPartnerExportEnabled+ 6�   �� *IsPartnerSelected+$ TwoWay=�$ PropertyChanged=� 6  �� *PartnersToExport+�� 5      $
��Left=�6   $��155,137,0,0q�64   $	��Top=�6I   ��6a   �� *PartnerLabelContent+�� 5      -   5        $ AgmtExportCheckBox��5      $��Agreement(s)��67   $
��Left=�6N   $��
20,183,0,0q�6i   $	��Top=�6}   {�6�   �� *IsAgmtExportEnabled+ 6�   �� *IsAgmtSelected+$ TwoWay=�$ PropertyChanged=� 6  �� *AgreementsToExport+�� 5      $
��Left=�6   $��155,187,0,0q�63   $	��Top=�6H   ��6`   �� *AgmtLabelContent+� 5"      .+�5#      �� -   5        $ OverwriteCheckBox��5#      $7��1Do you wish to Overwrite existing artifacts in IA��6:   $
��Left=�6v   $	��Top=�6�   $��20,250,-61.4,0q�6.  $��Medium"�6G   6�   �� *IsOverwriteSelected+$ TwoWay=�$ PropertyChanged=�{�6  �� *IsOverwriteEnabled+6G  5$      5%      5&      5'      5(      