   M S B A M L   `   `   `  ����G  CTpmMigration, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null�Mclr-namespace:Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration?Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration  5      c    ]Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.ExportArtifactsStatusPageView   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      ftpmMigrationMclr-namespace:Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration     5        � 	Resources  5
      m  gMicrosoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.MigrationStatusEnumToImagePathConverter +  'migrationStatusEnumToImagePathConverter_  YMicrosoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.NullToVisibilityConverter  nullToVisibilityConverterk  eMicrosoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.ProgressBarStateToVisibilityConverter ) %progressBarStateToVisibilityConverter��%W   &	        &	      &	 &       5   
   6?     5   
   61     5   
   6=   5      .��5      � -   5        $��Layout��5      $	��Top=�6   $
��Left=�61    � RowDefinitions 5   
   �� 5      $��90 �6   �� 5      $
��AUTO �6   .+�5   
   �� -   5        $��progressTextBlock��5      $��Center=�5      $��Center=�5      $��0��6/   $��30��6<   $	��700��6H   $��0,10,0,49.6q�6T   ��5   .   �� *ProgressBarStatusText+y�5      �� *ProgressVisible+5      � 5   
   -   5        $��pbStatus��5       � Minimum$ 0Q�5      $��0��5   &   $	��Top=�5      $��0,45,0,0q�6.   $��30��6@    � Maximum 5   "   �� *TotalArtifacts+ � Value 5      �� *
Progress+y�5      �� *ProgressVisible+5   @   &   System.Windows.Controls.DataGrid  5    
   -   5        $��ExportArtifcatsDataGrid��5"      $	��700��5!      $��1��6     StandardDataGrid#��[ 5#      $��	0,0,0,0.2q�6=   ��5       �� *ExportedItems+ �� Mode$ OneWay=�   Columns 5$      0  *System.Windows.Controls.DataGridTextColumn  5%      ,  &System.Windows.Controls.DataGridColumn   CanUserReorder .    CanUserResize . 	   CanUserSort	 . 
   
IsReadOnly
 .    Width5  /System.Windows.Controls.DataGridLengthConverter$	 60* 5'   �   X   RMicrosoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.DataGridExtensions   
HeaderName 5%   %   �� *ArtifactNameColumnHeader+  	Resources �� Source# [ 1	  +System.Windows.Controls.DataGridBoundColumn 	  Binding 5&   5   �� *ArtifactName+5'   �     5)       .  . 	 . 
 . $	 90* 5,   5    5)   %   �� *ArtifactTypeColumnHeader+# [  5*   5   �� *ArtifactType+5,   5   4
  .System.Windows.Controls.DataGridTemplateColumn
  5.      
 . $ 70 6;    6F   �� *MigrationStatusColumnHeader+# [  
  CellTemplate 5/      �� 50      .R�51      �� $��15��6�   ��6$   �� *ExportStatus+ �� 	Converter# [   � ToolTip 52   "   _� 53   &   $	��700��6�   y�6.   �� *ExportStatusText+# [ .��54   *   �� $
��Wrap=�6V   ��64   �� *ExportStatusText+6V   55   '   57      58      5:      5<      5=      5>      