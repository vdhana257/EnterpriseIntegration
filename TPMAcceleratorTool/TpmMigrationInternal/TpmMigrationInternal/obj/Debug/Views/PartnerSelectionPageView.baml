   M S B A M L   `   `   `  ����G  CTpmMigration, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null�Mclr-namespace:Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration?Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration  5      ^    XMicrosoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.PartnerSelectionPageView   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      ftpmMigrationMclr-namespace:Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration     5        � 	Resources  5	      _   YMicrosoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.BoolToVisibilityConverter   booleanToVisibiltyConverterd   ^Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.TextInputToVisibilityConverter " TextInputToVisibilityConverter_  YMicrosoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.NullToVisibilityConverter  NullToVisibilityConverter��%W   &	        &	      &	 &       5
   
   61     5   
   66     5   
   61   5      .��5      � -   5        $��Layout��5      $
��Left=�6   $	��Top=�64    � RowDefinitions 5   
   �� 5      $��40 �6   �� 5      $��* �6   .+�5   
   �� $	��570��6   $��30��6    $`��ZEnter specific partner name to search or press search button to display all partner names��5      
�������$��0,0,0,0q�5      $��0��6%   $��2��62   $
��Left=�5      $	��Top=�6/    �� TextAlignment$ Center=�6G   $��Normal$�6^   $��Light"�6q   $��13(�6�   y�5      `� 5       `� 	Converter# [ 6   .1�5      ��  �� ElementName$ PartnerFilterTextBox��6    �� Path$ Text.IsEmpty�6A   �� 5      $ PartnerFilterTextBox��6   $ 	IsFocused�6A   5      5       �� 5"   
   -   5        $��PartnerFilterTextBox��5#      
������ 
���� K�$
��Left=�5"   A   $��30��6\    �� TextWrapping$
 Wrap=�6h   $	��Top=�6|   $	��570��6�   $��0,0,0,0q�6�   $��0��6�   $��14(�6�   $��2��5#   .   ��5"   �   �� *PartnerFilter+5#   .   �� 5$   
   -   5        $��PartnerSearchButton��5$   ~   $��Search��6   $
��Left=�6"   $��	580,0,0,0q�6=   $��90��6P   $��0��6[   ���#0080c0$��30��6�   $	��Top=�6�   $��2��6�   ��6�   �� *SearchButtonClickCommand+6�   � 5%   
   $��1��6   $	��700��6   $��10,38.6,-10.2,-38.4q�6)   .+�5&      &   System.Windows.Controls.DataGrid  -   5          Name$ PartnerSelectionDataGrid��5'      $	��721��5&   K     StandardDataGrid#��[ 5(      $
��Left=�6;   $	��Top=�6V   "   VerticalScrollBarVisibility$ Visible=�5)      ��5&      �� *SelectionItems+	 �� Mode$	 OneWay=�y�5*      �� $ PartnerDataGridEnabled�
 �� 	Converter#
 [   �� FallbackValue$ Hidden��   Columns 5+      4  .System.Windows.Controls.DataGridTemplateColumn  5,      ,  &System.Windows.Controls.DataGridColumn   
IsReadOnly .    Width5  /System.Windows.Controls.DataGridLengthConverter$ 50 6?      HeaderTemplate 5-      �� 5.      .R�5/   "   ��  ��Name$ selectAllCheckBox��6+   $��15��6�   $��9,0,0,0q�6�   $��Center=�6�    d� 	IsChecked 6F   �� *DataContext.SelectAll+ �� RelativeSource ��  �� AncestorType"  6�   50         CellTemplate 53      �� 54      .R�55   "   �� $��Center=�6~   $��15��6�    6+   �� *
IsSelected+$	 TwoWay=� �� UpdateSourceTrigger$ PropertyChanged=�6�   56      59      0  *System.Windows.Controls.DataGridTextColumn  5;         CanUserReorder .    CanUserResize .    CanUserSort .  . $	 60* 5=   i   X	   RMicrosoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.DataGridExtensions 	  
HeaderName 5;   )   �� *PartnerNameColumnHeader+  	Resources �� Source# [ 1
  +System.Windows.Controls.DataGridBoundColumn 
  Binding 5<      �� *MigrationEntity.Name+5=   i     5?       .  .  .  . $	 90* 5B       5?   )   �� *ProfileCountColumnHeader+# [  5@      �� *Count+5B      5D      5E      5F      5G      